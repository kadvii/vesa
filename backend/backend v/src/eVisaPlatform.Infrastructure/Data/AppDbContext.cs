using eVisaPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eVisaPlatform.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<VisaApplication> VisaApplications => Set<VisaApplication>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketReply> TicketReplies => Set<TicketReply>();
    public DbSet<TravelConsultant> TravelConsultants => Set<TravelConsultant>();
    public DbSet<VisaAgent> VisaAgents => Set<VisaAgent>();
    public DbSet<GuaranteeRequest> GuaranteeRequests => Set<GuaranteeRequest>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasConversion<int>();
        });

        // VisaApplication
        modelBuilder.Entity<VisaApplication>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.VisaType).HasConversion<int>();
            e.Property(v => v.Status).HasConversion<int>();
            e.Property(v => v.ReviewedBy).HasMaxLength(200);
            e.HasOne(v => v.User)
             .WithMany(u => u.VisaApplications)
             .HasForeignKey(v => v.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // Structured applicant fields
            e.Property(v => v.DestinationCountry).HasMaxLength(150);
            e.Property(v => v.ApplicantFullName).HasMaxLength(200);
            e.Property(v => v.PassportNumber).HasMaxLength(50);
            e.Property(v => v.Nationality).HasMaxLength(100);

            // High-traffic: user's applications ordered by SubmissionDate (see VisaApplicationRepository)
            e.HasIndex(v => new { v.UserId, v.SubmissionDate })
                .IsDescending(false, true);
            // Admin/report-style filters by Status; SubmissionDate supports sorts and range scans
            e.HasIndex(v => new { v.Status, v.SubmissionDate })
                .IsDescending(false, true);
        });

        // Document
        modelBuilder.Entity<Document>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.FileName).HasMaxLength(300).IsRequired();
            e.Property(d => d.FilePath).HasMaxLength(500).IsRequired();
            e.Property(d => d.FileType).HasConversion<int>();
            e.HasOne(d => d.Application)
             .WithMany(v => v.Documents)
             .HasForeignKey(d => d.ApplicationId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(r => r.Token).IsUnique();
            e.HasOne(r => r.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r => new { r.UserId, r.IsRevoked });
        });

        // Notification
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Title).HasMaxLength(200).IsRequired();
            e.Property(n => n.Message).HasMaxLength(1000).IsRequired();
            e.HasOne(n => n.User)
             .WithMany(u => u.Notifications)
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // FamilyMember
        modelBuilder.Entity<FamilyMember>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.FullName).HasMaxLength(200).IsRequired();
            e.Property(f => f.PassportNumber).HasMaxLength(50).IsRequired();
            e.HasOne(f => f.Application)
             .WithMany(v => v.FamilyMembers)
             .HasForeignKey(f => f.ApplicationId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // SupportTicket & Reply
        modelBuilder.Entity<SupportTicket>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Title).HasMaxLength(200).IsRequired();
            e.Property(t => t.Status).HasConversion<int>();
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<TicketReply>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Ticket)
             .WithMany(t => t.Replies)
             .HasForeignKey(r => r.TicketId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Sender).WithMany().HasForeignKey(r => r.SenderId).OnDelete(DeleteBehavior.Restrict);
        });

        // TravelConsultant
        modelBuilder.Entity<TravelConsultant>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.FullName).HasMaxLength(200).IsRequired();
        });

        // VisaAgent
        modelBuilder.Entity<VisaAgent>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Name).HasMaxLength(200).IsRequired();
        });

        // GuaranteeRequest
        modelBuilder.Entity<GuaranteeRequest>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Status).HasConversion<int>();
            e.HasOne(g => g.Application)
             .WithOne(v => v.GuaranteeRequest)
             .HasForeignKey<GuaranteeRequest>(g => g.ApplicationId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Ensure VisaApplication relates to Consultant and Agent
        modelBuilder.Entity<VisaApplication>(e =>
        {
            e.HasOne(v => v.TravelConsultant)
             .WithMany(c => c.AssignedApplications)
             .HasForeignKey(v => v.TravelConsultantId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(v => v.VisaAgent)
             .WithMany(a => a.AssignedApplications)
             .HasForeignKey(v => v.VisaAgentId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // Payment
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            e.Property(p => p.Currency).HasMaxLength(3);
            e.Property(p => p.Status).HasConversion<int>();
            e.Property(p => p.Method).HasConversion<int>();
            e.Property(p => p.TransactionReference).HasMaxLength(100);
            e.HasOne(p => p.Application)
             .WithMany()
             .HasForeignKey(p => p.ApplicationId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.User)
             .WithMany()
             .HasForeignKey(p => p.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(p => new { p.ApplicationId, p.CreatedAt }).IsDescending(false, true);
            e.HasIndex(p => new { p.UserId, p.CreatedAt }).IsDescending(false, true);
        });

        // Appointment
        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Location).HasMaxLength(500).IsRequired();
            e.Property(a => a.Status).HasConversion<int>();
            e.HasOne(a => a.Application)
             .WithMany()
             .HasForeignKey(a => a.ApplicationId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.User)
             .WithMany()
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLog (append-only, no FK constraints)
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Action).HasMaxLength(100).IsRequired();
            e.Property(l => l.EntityName).HasMaxLength(100).IsRequired();
            e.Property(l => l.UserEmail).HasMaxLength(200);
            e.Property(l => l.IpAddress).HasMaxLength(50);
            e.HasIndex(l => l.CreatedAt);
            e.HasIndex(l => l.UserId);
        });

        // SystemSetting
        modelBuilder.Entity<SystemSetting>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Key).HasMaxLength(200).IsRequired();
            e.HasIndex(s => s.Key).IsUnique();
            e.Property(s => s.Value).HasMaxLength(2000).IsRequired();
            e.Property(s => s.Description).HasMaxLength(500);
        });
    }
}
