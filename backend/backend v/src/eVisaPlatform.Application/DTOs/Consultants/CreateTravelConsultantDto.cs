namespace eVisaPlatform.Application.DTOs.Consultants;

public class CreateTravelConsultantDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class TravelConsultantResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

/// <summary>DTO for a user booking a session with a consultant</summary>
public class BookConsultantDto
{
    public Guid ApplicationId { get; set; }
    public string? PreferredDate { get; set; }   // ISO 8601 date hint, optional
    public string? Notes { get; set; }
}

public class BookConsultantResponseDto
{
    public string Message { get; set; } = string.Empty;
    public Guid ConsultantId { get; set; }
    public Guid ApplicationId { get; set; }
}
