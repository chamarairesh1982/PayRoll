namespace Payroll.Domain.Common;

/// <summary>
/// Interface for entities that belong to a specific tenant.
/// All tenant-scoped entities must implement this interface.
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// The tenant identifier. Must be set for all tenant-scoped entities.
    /// </summary>
    Guid TenantId { get; set; }
}
