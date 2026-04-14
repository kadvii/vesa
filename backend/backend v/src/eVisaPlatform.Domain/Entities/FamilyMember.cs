namespace eVisaPlatform.Domain.Entities;

public class FamilyMember
{
    public Guid Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Relationship { get; set; } = string.Empty;

    // Navigation
    public VisaApplication Application { get; set; } = null!;
}
