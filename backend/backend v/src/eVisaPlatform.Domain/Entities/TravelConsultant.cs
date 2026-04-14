namespace eVisaPlatform.Domain.Entities;

public class TravelConsultant
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // Navigation
    public ICollection<VisaApplication> AssignedApplications { get; set; } = new List<VisaApplication>();
}
