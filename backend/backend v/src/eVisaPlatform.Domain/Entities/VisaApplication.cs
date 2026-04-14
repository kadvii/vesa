using eVisaPlatform.Domain.Enums;

namespace eVisaPlatform.Domain.Entities;

public class VisaApplication
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public VisaType VisaType { get; set; }
    public VisaStatus Status { get; set; } = VisaStatus.Pending;
    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? Notes { get; set; }

    // Structured applicant fields (previously packed into Notes string)
    public string? DestinationCountry { get; set; }
    public string? ApplicantFullName { get; set; }
    public string? PassportNumber { get; set; }
    public string? Nationality { get; set; }
    public DateTime? IntendedTravelDate { get; set; }

    // Foreign Keys for Assignment
    public Guid? TravelConsultantId { get; set; }
    public Guid? VisaAgentId { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
    
    public TravelConsultant? TravelConsultant { get; set; }
    public VisaAgent? VisaAgent { get; set; }
    public GuaranteeRequest? GuaranteeRequest { get; set; }
}
