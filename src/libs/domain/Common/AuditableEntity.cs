namespace Payroll.Domain.Common;

/// <summary>
/// Base class for all entities that require audit tracking.
/// Includes created/updated timestamps, user tracking, and optimistic concurrency.
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UTC timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Username or identifier of the user who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Username or identifier of the user who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Row version for optimistic concurrency control.
    /// EF Core will automatically manage this as a timestamp.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Indicates whether the entity is active (soft delete).
    /// </summary>
    public bool IsActive { get; set; } = true;
}
