namespace eVisaPlatform.Domain.Entities;

public class VisaAgent
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public int PerformanceScore { get; set; }

    // Navigation
    public ICollection<VisaApplication> AssignedApplications { get; set; } = new List<VisaApplication>();
}
