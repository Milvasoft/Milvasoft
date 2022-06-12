using Microsoft.AspNetCore.Identity;
using Milvasoft.Core.EntityBase.Abstract;
using System;

namespace Milvasoft.Core.EntityBase.MultiTenancy;

/// <summary>
/// Represents a Tenant of the application.
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TUserKey"></typeparam>
public abstract class MilvaTenant<TUser, TUserKey> : MilvaBaseTenant<TUserKey, TenantId>, IFullAuditableWithCustomUser<TUser, TUserKey, TenantId>
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
    /// Deleter user navigation property.
    /// </summary>
    public TUser DeleterUser { get; set; }

    /// <summary>
    /// Modifier user navigation property.
    /// </summary>
    public TUser LastModifierUser { get; set; }

    /// <summary>
    /// Creator user navigation property.
    /// </summary>
    public TUser CreatorUser { get; set; }

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

/// <summary>
/// Represents a Tenant of the application.
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TUserKey"></typeparam>
public abstract class MilvaIdentityTenant<TUser, TUserKey> : MilvaBaseTenant<TUserKey, TenantId>, IFullAuditable<TUser, TUserKey, TenantId>
where TUser : IdentityUser<TUserKey>
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
    /// Deleter user navigation property.
    /// </summary>
    public TUser DeleterUser { get; set; }

    /// <summary>
    /// Modifier user navigation property.
    /// </summary>
    public TUser LastModifierUser { get; set; }

    /// <summary>
    /// Creator user navigation property.
    /// </summary>
    public TUser CreatorUser { get; set; }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    protected MilvaIdentityTenant()
    {
        Id = TenantId.NewTenantId();
        IsActive = true;
    }

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    /// <param name="tenancyName">UNIQUE name of this Tenant</param>
    /// <param name="branchNo"></param>
    protected MilvaIdentityTenant(string tenancyName, int branchNo)
    {
        Id = new TenantId(tenancyName, branchNo);
        _tenancyName = tenancyName;
        _branchNo = branchNo;
        IsActive = true;
    }
}