using eVisaPlatform.Application.Interfaces;
using eVisaPlatform.Domain.Entities;
using eVisaPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eVisaPlatform.Infrastructure.Data;

/// <summary>
/// Development seed data for dashboard / frontend testing. Idempotent: runs only when the Users table is empty.
/// Default password for all seeded accounts: ChangeMe!123
/// </summary>
public static class DatabaseSeeder
{
    public const string DefaultSeedPassword = "ChangeMe!123";

    public static async Task SeedAsync(AppDbContext db, IPasswordService passwordService, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
            return;

        var utc = DateTime.UtcNow;
        var adminEmail = "admin@visaz.local";
        var employeeEmail = "employee@visaz.local";

        var admin = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Amira Hassan",
            Email = adminEmail,
            PasswordHash = passwordService.HashPassword(DefaultSeedPassword),
            Role = UserRole.Admin,
            CreatedAt = utc.AddMonths(-14)
        };

        var employee = new User
        {
            Id = Guid.NewGuid(),
            FullName = "James Okonkwo",
            Email = employeeEmail,
            PasswordHash = passwordService.HashPassword(DefaultSeedPassword),
            Role = UserRole.Employee,
            CreatedAt = utc.AddMonths(-11)
        };

        var applicants = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Sofia Martinez",
                Email = "sofia.martinez@example.com",
                PasswordHash = passwordService.HashPassword(DefaultSeedPassword),
                Role = UserRole.User,
                CreatedAt = utc.AddMonths(-9)
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Chen Wei",
                Email = "chen.wei@example.com",
                PasswordHash = passwordService.HashPassword(DefaultSeedPassword),
                Role = UserRole.User,
                CreatedAt = utc.AddMonths(-8)
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Olivia Brown",
                Email = "olivia.brown@example.com",
                PasswordHash = passwordService.HashPassword(DefaultSeedPassword),
                Role = UserRole.User,
                CreatedAt = utc.AddMonths(-7)
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Raj Patel",
                Email = "raj.patel@example.com",
                PasswordHash = passwordService.HashPassword(DefaultSeedPassword),
                Role = UserRole.User,
                CreatedAt = utc.AddMonths(-6)
            },
            new User
            {
                Id = Guid.NewGuid(),
                FullName = "Elena Popescu",
                Email = "elena.popescu@example.com",
                PasswordHash = passwordService.HashPassword(DefaultSeedPassword),
                Role = UserRole.User,
                CreatedAt = utc.AddMonths(-5)
            }
        };

        await db.Users.AddRangeAsync(new[] { admin, employee }.Concat(applicants), ct);

        var consultants = Enumerable.Range(0, 10).Select(i => new TravelConsultant
        {
            Id = Guid.NewGuid(),
            FullName = ConsultantNames[i],
            Email = $"consultant{i + 1:D2}@visaz-travel.com",
            Phone = $"+1-415-555-{1000 + i:D4}"
        }).ToList();

        var agents = Enumerable.Range(0, 10).Select(i => new VisaAgent
        {
            Id = Guid.NewGuid(),
            Name = AgentNames[i],
            Company = AgentCompanies[i],
            PerformanceScore = 72 + i * 2 + (i % 3)
        }).ToList();

        await db.TravelConsultants.AddRangeAsync(consultants, ct);
        await db.VisaAgents.AddRangeAsync(agents, ct);

        var applicantIds = applicants.Select(a => a.Id).ToList();
        var visaTypes = (VisaType[])Enum.GetValues(typeof(VisaType));

        // 20 applications: 7 Pending, 7 Approved, 6 Rejected — distributed across applicants
        var statusPattern = new[]
        {
            VisaStatus.Pending, VisaStatus.Pending, VisaStatus.Approved, VisaStatus.Rejected,
            VisaStatus.Pending, VisaStatus.Approved, VisaStatus.Approved, VisaStatus.Rejected,
            VisaStatus.Pending, VisaStatus.Rejected, VisaStatus.Approved, VisaStatus.Pending,
            VisaStatus.Approved, VisaStatus.Rejected, VisaStatus.Pending, VisaStatus.Approved,
            VisaStatus.Rejected, VisaStatus.Pending, VisaStatus.Approved, VisaStatus.Rejected
        };

        var applications = new List<VisaApplication>();
        for (var i = 0; i < statusPattern.Length; i++)
        {
            var status = statusPattern[i];
            var userId = applicantIds[i % applicantIds.Count];
            var daysAgo = 85 - i * 3;
            var submission = utc.Date.AddDays(-daysAgo).AddHours(9 + (i % 8)).AddMinutes((i * 7) % 60);

            var application = new VisaApplication
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VisaType = visaTypes[i % visaTypes.Length],
                Status = status,
                SubmissionDate = submission,
                TravelConsultantId = consultants[i % consultants.Count].Id,
                VisaAgentId = agents[(i + 2) % agents.Count].Id
            };

            switch (status)
            {
                case VisaStatus.Approved:
                    application.ReviewedBy = adminEmail;
                    application.ReviewDate = submission.AddDays(2 + (i % 5)).AddHours(3);
                    application.Notes = "All supporting documents verified. Visa granted per embassy checklist.";
                    break;
                case VisaStatus.Rejected:
                    application.ReviewedBy = employeeEmail;
                    application.ReviewDate = submission.AddDays(1 + (i % 4)).AddHours(2);
                    application.Notes = i % 2 == 0
                        ? "Incomplete financial proof; applicant may reapply with updated bank statements."
                        : "Passport validity does not meet destination minimum stay requirement.";
                    break;
                default:
                    application.Notes = i % 3 == 0 ? "Awaiting biometric appointment confirmation." : null;
                    break;
            }

            applications.Add(application);
        }

        await db.VisaApplications.AddRangeAsync(applications, ct);
        await db.SaveChangesAsync(ct);
    }

    private static readonly string[] ConsultantNames =
    {
        "Jordan Blake", "Priya Nair", "Marcus Lindholm", "Fatima Al-Rashid", "Diego Romero",
        "Hannah Vogel", "Kwame Asante", "Yuki Tanaka", "Ingrid Svensson", "Luca Ferretti"
    };

    private static readonly string[] AgentNames =
    {
        "Northwind Visa Desk", "Atlas Mobility", "Blue Horizon Processing", "Summit Travel Legal",
        "Pacific Gateway Services", "EuroLink Visa Support", "Cedar Ridge Immigration", "Nova Expedite",
        "Harborfront Consultants", "Global Reach Visa Co."
    };

    private static readonly string[] AgentCompanies =
    {
        "Northwind Ltd.", "Atlas Group", "Blue Horizon Inc.", "Summit Partners", "Pacific Gateway LLC",
        "EuroLink SA", "Cedar Ridge LLC", "Nova Expedite Ltd.", "Harborfront BV", "Global Reach Holdings"
    };
}
