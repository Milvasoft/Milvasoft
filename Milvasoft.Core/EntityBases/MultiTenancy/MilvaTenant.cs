using Microsoft.AspNetCore.Identity;
using Milvasoft.Core.EntityBases.Abstract;

namespace Milvasoft.Core.EntityBases.MultiTenancy;

/// <summary>
/// Represents a Tenant of the application.
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TUserKey"></typeparam>
public abstract class MilvaTenant<TUser, TUserKey> : MilvaBaseTenant<TenantId>, IFullAuditable<TenantId>
where TUser : class, IBaseEntity<TUserKey>
where TUserKey : struct, IEquatable<TUserKey>
{
    private readonly string _tenancyName;
    private readonly int _branchNo;

    /// <summary>
    /// Id of tenant.
    /// </summary>
    public override TenantId Id
    {
        get => base.Id;
    }

    /// <summary>
    /// Tenancy name of tenant.
    /// </summary>
    public override string TenancyName
    {
        get => _tenancyName;
    }

    /// <summary>
    /// Display name of the Tenant.
    /// </summary>
    public virtual int BranchNo
    {
        get => _branchNo;
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
        _tenancyName = tenancyName;
        _branchNo = branchNo;
        IsActive = true;
    }
}