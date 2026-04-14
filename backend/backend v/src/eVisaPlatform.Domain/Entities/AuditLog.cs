namespace eVisaPlatform.Domain.Entities;

/// <summary>Immutable audit trail entry for compliance and change tracking.</summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>The action performed (e.g., Create, Update, Delete, Login).</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>The entity/resource type affected (e.g., VisaApplication, Payment).</summary>
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }

    /// <summary>JSON snapshot of old values (for updates).</summary>
    public string? OldValues { get; set; }

    /// <summary>JSON snapshot of new values.</summary>
    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
