namespace eVisaPlatform.Application.DTOs.Family;

public class CreateFamilyMemberDto
{
    public Guid ApplicationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Relationship { get; set; } = string.Empty;
}
