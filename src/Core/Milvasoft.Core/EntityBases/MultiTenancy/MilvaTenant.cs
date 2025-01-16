namespace Milvasoft.Core.EntityBases.MultiTenancy;

/// <summary>
/// Represents a Tenant of the application.
/// </summary>
public abstract class MilvaTenant : MilvaBaseTenant<TenantId>, IFullAuditable<TenantId>
{
    /// <summary>
    /// Id of tenant.
    /// </summary>
    public override TenantId Id
    {
        get => base.Id;
        set => base.Id = value;
    }

    /// <summary>
    /// Display name of the Tenant.
    /// </summary>
    public virtual int BranchNo
    {
        get => base.Id.BranchNo;
    }

    /// <summary>
    /// Represents Tenant's subscription expire date.
    /// </summary>
    public DateTime SubscriptionExpireDate { get; set; }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    protected MilvaTenant()
    {
        Id = TenantId.NewTenantId();
        TenancyName = Id.TenancyName;
        IsActive = true;
    }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="tenancyName">UNIQUE name of this Tenant</param>
    /// <param name="branchNo"></param>
    protected MilvaTenant(string tenancyName, int branchNo)
    {
        Id = new TenantId(tenancyName, branchNo);
        TenancyName = tenancyName;
        IsActive = true;
    }
}