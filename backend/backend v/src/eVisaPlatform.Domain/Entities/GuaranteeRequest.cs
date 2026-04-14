using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Domain.Entities;

public class GuaranteeRequest
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public int RiskScore { get; set; }
    public GuaranteeStatus Status { get; set; } = GuaranteeStatus.Pending;
    public string? Notes { get; set; }

    // Navigation
    public VisaApplication Application { get; set; } = null!;
}
