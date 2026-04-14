using AutoMapper;
using eVisaPlatform.Application.Common;
using eVisaPlatform.Application.DTOs.Visa;
using eVisaPlatform.Application.Email;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;
using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Application.Services;

public class VisaApplicationService : IVisaApplicationService
{
    private readonly IUnitOfWork        _unitOfWork;
    private readonly IMapper            _mapper;
    private readonly IDateTimeProvider  _clock;
    private readonly IEmailService      _email;

    public VisaApplicationService(
        IUnitOfWork       unitOfWork,
        IMapper           mapper,
        IDateTimeProvider clock,
        IEmailService     email)
    {
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
        _clock      = clock;
        _email      = email;
    }

    private static (int Page, int PageSize) NormalizePaging(int page, int pageSize)
    {
        var p = page < 1 ? 1 : page;
        var s = pageSize < 1 ? 20 : Math.Min(pageSize, 200);
        return (p, s);
    }

    // ── GET ALL (Admin/Employee) ──────────────────────────────────────────────
    public async Task<ApiResponse<PagedResult<VisaApplicationResponseDto>>> GetAllAsync(int page, int pageSize)
    {
        var (p, s) = NormalizePaging(page, pageSize);
        var (items, total) = await _unitOfWork.VisaApplications.GetAllWithDetailsPagedAsync(p, s);
        var dto = _mapper.Map<IReadOnlyList<VisaApplicationResponseDto>>(items);
        return ApiResponse<PagedResult<VisaApplicationResponseDto>>.Ok(
            new PagedResult<VisaApplicationResponseDto>
            {
                Items = dto,
                Page = p,
                PageSize = s,
                TotalCount = total
            });
    }

    // ── GET MY REQUESTS (Authenticated User) ─────────────────────────────────
    public async Task<ApiResponse<PagedResult<VisaApplicationResponseDto>>> GetMyRequestsAsync(
        Guid userId, int page, int pageSize)
    {
        var (p, s) = NormalizePaging(page, pageSize);
        var (items, total) = await _unitOfWork.VisaApplications.GetByUserIdPagedAsync(userId, p, s);
        var dto = _mapper.Map<IReadOnlyList<VisaApplicationResponseDto>>(items);
        return ApiResponse<PagedResult<VisaApplicationResponseDto>>.Ok(
            new PagedResult<VisaApplicationResponseDto>
            {
                Items = dto,
                Page = p,
                PageSize = s,
                TotalCount = total
            });
    }

    // ── GET BY ID (Owner or Admin) ────────────────────────────────────────────
    public async Task<ApiResponse<VisaApplicationResponseDto>> GetByIdAsync(
        Guid id, Guid requestingUserId, bool isAdmin)
    {
        var app = await _unitOfWork.VisaApplications.GetByIdWithDetailsAsync(id);
        if (app == null)
            return ApiResponse<VisaApplicationResponseDto>.Fail("Application not found.");

        // Non-admins can only view their own applications
        if (!isAdmin && app.UserId != requestingUserId)
            return ApiResponse<VisaApplicationResponseDto>.Fail("You are not authorised to view this application.");

        return ApiResponse<VisaApplicationResponseDto>.Ok(
            _mapper.Map<VisaApplicationResponseDto>(app));
    }

    // ── APPLY (Create) ────────────────────────────────────────────────────────
    public async Task<ApiResponse<VisaApplicationResponseDto>> CreateAsync(
        Guid userId, CreateVisaApplicationDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponse<VisaApplicationResponseDto>.Fail("User not found.");

        var application = new VisaApplication
        {
            Id                  = Guid.NewGuid(),
            UserId              = userId,
            VisaType            = dto.VisaType,
            Status              = VisaStatus.Pending,
            SubmissionDate      = _clock.UtcNow,
            // Map structured fields directly — no more Notes packing
            DestinationCountry  = dto.DestinationCountry?.Trim(),
            ApplicantFullName   = dto.ApplicantFullName?.Trim(),
            PassportNumber      = dto.PassportNumber?.Trim(),
            Nationality         = dto.Nationality?.Trim(),
            IntendedTravelDate  = dto.IntendedTravelDate,
            Notes               = dto.Notes?.Trim()
        };
        await _unitOfWork.VisaApplications.AddAsync(application);

        // Send in-app notification
        await _unitOfWork.Notifications.AddAsync(new Notification
        {
            Id        = Guid.NewGuid(),
            UserId    = userId,
            Title     = "Visa Application Submitted",
            Message   = $"Your {dto.VisaType} visa application has been submitted and is pending review.",
            CreatedAt = _clock.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        // Re-fetch to populate navigation properties for response
        var created = await _unitOfWork.VisaApplications.GetByIdWithDetailsAsync(application.Id);

        // Send submission confirmation email (fire-and-forget — failure is logged, not thrown)
        var userEmail = created?.User?.Email;
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            var html = VisaEmailTemplates.Submitted(
                applicantName:   dto.ApplicantFullName ?? created?.User?.FullName ?? "المتقدم",
                visaType:        dto.VisaType.ToString(),
                destination:     dto.DestinationCountry ?? "—",
                applicationId:   application.Id,
                submissionDate:  application.SubmissionDate);

            _ = _email.SendAsync(
                toAddress: userEmail,
                toName:    dto.ApplicantFullName ?? "المتقدم",
                subject:   "✅ تم استلام طلب تأشيرتك — Visa Z",
                htmlBody:  html);
        }

        return ApiResponse<VisaApplicationResponseDto>.Ok(
            _mapper.Map<VisaApplicationResponseDto>(created),
            "Visa application submitted successfully.");
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────
    public async Task<ApiResponse<VisaApplicationResponseDto>> UpdateAsync(
        Guid id, Guid userId, UpdateVisaApplicationDto dto)
    {
        var app = await _unitOfWork.VisaApplications.GetByIdWithDetailsAsync(id);
        if (app == null)
            return ApiResponse<VisaApplicationResponseDto>.Fail("Application not found.");
        if (app.UserId != userId)
            return ApiResponse<VisaApplicationResponseDto>.Fail("You are not authorised to update this application.");
        if (app.Status != VisaStatus.Pending)
            return ApiResponse<VisaApplicationResponseDto>.Fail("Only Pending applications can be updated.");

        if (dto.VisaType.HasValue) app.VisaType = dto.VisaType.Value;
        if (dto.Notes != null)     app.Notes    = dto.Notes;

        _unitOfWork.VisaApplications.Update(app);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<VisaApplicationResponseDto>.Ok(
            _mapper.Map<VisaApplicationResponseDto>(app));
    }

    // ── APPROVE (Admin/Employee) ──────────────────────────────────────────────
    public async Task<ApiResponse<VisaApplicationResponseDto>> ApproveAsync(
        Guid id, string reviewerEmail, ReviewVisaApplicationDto dto)
    {
        var app = await _unitOfWork.VisaApplications.GetByIdWithDetailsAsync(id);
        if (app == null)
            return ApiResponse<VisaApplicationResponseDto>.Fail("Application not found.");
        if (app.Status != VisaStatus.Pending)
            return ApiResponse<VisaApplicationResponseDto>.Fail("Only Pending applications can be approved.");

        app.Status     = VisaStatus.Approved;
        app.ReviewedBy = reviewerEmail;
        app.ReviewDate = _clock.UtcNow;
        if (dto.Notes != null) app.Notes = dto.Notes;

        _unitOfWork.VisaApplications.Update(app);

        await _unitOfWork.Notifications.AddAsync(new Notification
        {
            Id        = Guid.NewGuid(),
            UserId    = app.UserId,
            Title     = "Visa Application Approved ✅",
            Message   = $"Congratulations! Your {app.VisaType} visa application has been approved.",
            CreatedAt = _clock.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        // Send Approved email (non-blocking — failure logged, never thrown)
        var userEmail = app.User?.Email;
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            var html = VisaEmailTemplates.Approved(
                applicantName: app.ApplicantFullName ?? app.User?.FullName ?? "المتقدم",
                visaType:      app.VisaType.ToString(),
                destination:   app.DestinationCountry ?? "—",
                applicationId: app.Id,
                reviewerName:  reviewerEmail,
                reviewDate:    app.ReviewDate!.Value);

            _ = _email.SendAsync(
                toAddress: userEmail,
                toName:    app.ApplicantFullName ?? "المتقدم",
                subject:   "✅ تهانينا! تمت الموافقة على تأشيرتك — Visa Z",
                htmlBody:  html);
        }

        return ApiResponse<VisaApplicationResponseDto>.Ok(
            _mapper.Map<VisaApplicationResponseDto>(app), "Application approved.");
    }

    // ── REJECT (Admin/Employee) ───────────────────────────────────────────────
    public async Task<ApiResponse<VisaApplicationResponseDto>> RejectAsync(
        Guid id, string reviewerEmail, ReviewVisaApplicationDto dto)
    {
        var app = await _unitOfWork.VisaApplications.GetByIdWithDetailsAsync(id);
        if (app == null)
            return ApiResponse<VisaApplicationResponseDto>.Fail("Application not found.");
        if (app.Status != VisaStatus.Pending)
            return ApiResponse<VisaApplicationResponseDto>.Fail("Only Pending applications can be rejected.");

        app.Status     = VisaStatus.Rejected;
        app.ReviewedBy = reviewerEmail;
        app.ReviewDate = _clock.UtcNow;
        if (dto.Notes != null) app.Notes = dto.Notes;

        _unitOfWork.VisaApplications.Update(app);

        await _unitOfWork.Notifications.AddAsync(new Notification
        {
            Id        = Guid.NewGuid(),
            UserId    = app.UserId,
            Title     = "Visa Application Decision",
            Message   = $"Your {app.VisaType} visa application has been rejected. Reason: {dto.Notes}",
            CreatedAt = _clock.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        // Send Rejected email (non-blocking — failure logged, never thrown)
        var userEmail = app.User?.Email;
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            var html = VisaEmailTemplates.Rejected(
                applicantName: app.ApplicantFullName ?? app.User?.FullName ?? "المتقدم",
                visaType:      app.VisaType.ToString(),
                destination:   app.DestinationCountry ?? "—",
                applicationId: app.Id,
                reason:        dto.Notes ?? string.Empty,
                reviewDate:    app.ReviewDate!.Value);

            _ = _email.SendAsync(
                toAddress: userEmail,
                toName:    app.ApplicantFullName ?? "المتقدم",
                subject:   "إشعار بشأن طلب تأشيرتك — Visa Z",
                htmlBody:  html);
        }

        return ApiResponse<VisaApplicationResponseDto>.Ok(
            _mapper.Map<VisaApplicationResponseDto>(app), "Application rejected.");
    }

    // ── STATS (Admin/Employee dashboard) ─────────────────────────────────────
    public async Task<VisaStatsDto> GetStatsAsync()
    {
        var all = (await _unitOfWork.VisaApplications.GetAllAsync()).ToList();
        return new VisaStatsDto
        {
            Total    = all.Count,
            Pending  = all.Count(a => a.Status == VisaStatus.Pending),
            Approved = all.Count(a => a.Status == VisaStatus.Approved),
            Rejected = all.Count(a => a.Status == VisaStatus.Rejected),
        };
    }
}
