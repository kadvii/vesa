using AutoMapper;
using eVisaPlatform.Application.DTOs.Appointment;
using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;
using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Validates the application belongs to the requesting user, ensures the
    /// scheduled time is in the future, then creates an Appointment record.
    /// </summary>
    public async Task<AppointmentResponseDto> ScheduleAsync(Guid userId, CreateAppointmentDto dto)
    {
        // 1. Validate application ownership
        var app = await _unitOfWork.VisaApplications.GetByIdAsync(dto.ApplicationId)
            ?? throw new KeyNotFoundException("Visa application not found.");

        if (app.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this application.");

        // 2. Ensure appointment is in the future
        if (dto.ScheduledAt <= DateTime.UtcNow)
            throw new ArgumentException("Appointment must be scheduled for a future date/time.");

        // 3. Prevent duplicate active appointments for the same application
        var existing = await _unitOfWork.Appointments.GetByApplicationIdAsync(dto.ApplicationId);
        if (existing.Any(a => a.Status == AppointmentStatus.Scheduled))
            throw new InvalidOperationException("An active appointment already exists for this application.");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            ApplicationId = dto.ApplicationId,
            UserId = userId,
            ScheduledAt = dto.ScheduledAt,
            Location = dto.Location,
            Notes = dto.Notes,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Appointments.AddAsync(appointment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<AppointmentResponseDto>(appointment);
    }

    /// <summary>Fetch appointment details by its unique ID.</summary>
    public async Task<AppointmentResponseDto> GetByIdAsync(Guid id)
    {
        var appt = await _unitOfWork.Appointments.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Appointment not found.");
        return _mapper.Map<AppointmentResponseDto>(appt);
    }

    /// <summary>
    /// Cancels an appointment. Only the owner or an admin can cancel.
    /// Already-cancelled appointments return an error.
    /// </summary>
    public async Task CancelAsync(Guid userId, Guid appointmentId, bool isAdmin = false)
    {
        var appt = await _unitOfWork.Appointments.GetByIdAsync(appointmentId)
            ?? throw new KeyNotFoundException("Appointment not found.");

        // Access check: must be owner or admin
        if (!isAdmin && appt.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to cancel this appointment.");

        if (appt.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled.");

        appt.Status = AppointmentStatus.Cancelled;
        _unitOfWork.Appointments.Update(appt);
        await _unitOfWork.SaveChangesAsync();
    }
}
