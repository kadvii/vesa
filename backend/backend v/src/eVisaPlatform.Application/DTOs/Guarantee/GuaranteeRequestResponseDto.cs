namespace eVisaPlatform.Application.DTOs.Guarantee;

public class GuaranteeRequestResponseDto
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public int RiskScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
