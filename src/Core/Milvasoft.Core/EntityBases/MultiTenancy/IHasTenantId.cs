namespace Milvasoft.Core.EntityBases.MultiTenancy;

/// <summary>
/// Tenant id column for single database multi tenancy scenarios.
/// </summary>
public interface IHasTenantId
{
    /// <summary>
    /// TenantId of record.
    /// </summary>
    TenantId TenantId { get; set; }
}
