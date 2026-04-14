using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Application.DTOs.Visa;

public class CreateVisaApplicationDto
{
    /// <summary>Visa category (matches dashboard: tourist, business, … as enum value).</summary>
    public VisaType VisaType { get; set; }

    /// <summary>Optional free-text notes from the applicant.</summary>
    public string? Notes { get; set; }

    /// <summary>Destination country (Arabic or Latin label from the UI).</summary>
    public string? DestinationCountry { get; set; }

    /// <summary>Planned travel date from the booking form.</summary>
    public DateTime? IntendedTravelDate { get; set; }

    public string? ApplicantFullName { get; set; }
    public string? PassportNumber { get; set; }
    public string? Nationality { get; set; }
}

public class UpdateVisaApplicationDto
{
    public VisaType? VisaType { get; set; }
    public string? Notes { get; set; }
}

public class ReviewVisaApplicationDto
{
    public string? Notes { get; set; }
}

/// <summary>
/// Unified body for the Admin PATCH /api/visa/{id}/status endpoint.
/// status must be "Approved" or "Rejected".
/// </summary>
public class UpdateVisaStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class VisaApplicationResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string VisaType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? Notes { get; set; }

    // Structured applicant fields (Fix #6)
    public string? DestinationCountry { get; set; }
    public string? ApplicantFullName { get; set; }
    public string? Nationality { get; set; }
    public DateTime? IntendedTravelDate { get; set; }
    // PassportNumber intentionally omitted from list DTO (Fix #13) — exposed only in detail view
}

/// <summary>Lightweight stats for admin/employee dashboard cards (Fix #16).</summary>
public class VisaStatsDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
}
