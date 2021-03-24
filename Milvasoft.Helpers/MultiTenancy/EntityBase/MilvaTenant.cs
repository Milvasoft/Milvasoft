using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;
using System.Collections.Generic;

namespace Milvasoft.Helpers.MultiTenancy.EntityBase
{
    /// <summary>
    /// Represents a Tenant of the application.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TUserKey"></typeparam>
    public abstract class MilvaTenant<TUser, TUserKey> : MilvaTenantBase<TUserKey, TenantId>, IFullAuditable<TUser, TUserKey, TenantId>
    where TUser : IdentityUser<TUserKey>
    where TUserKey : struct, IEquatable<TUserKey>
    {
        private string _tenancyName;
        private int _branchNo;

        /// <summary>
        /// Id of tenant.
        /// </summary>
        public override TenantId Id
        {
            get => base.Id;
            set => base.Id = new TenantId(_tenancyName, _branchNo);
        }

        /// <summary>
        /// Tenancy name of tenant.
        /// </summary>
        public override string TenancyName
        {
            get=> Id.TenancyName;
        }

        /// <summary>
        /// Display name of the Tenant.
        /// </summary>
        public virtual int BranchNo
        {
            get => Id.BranchNo;
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
        /// Gets or sets additional tenant items.
        /// </summary>
        public Dictionary<string, object> Items { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates a new tenant.
        /// </summary>
        /// <param name="tenancyName">UNIQUE name of this Tenant</param>
        /// <param name="branchNo"></param>
        protected MilvaTenant(string tenancyName, int branchNo)
        {
            _tenancyName = tenancyName;
            _branchNo = branchNo;
            Id = new TenantId(tenancyName, branchNo);
            IsActive = true;
        }
    }
}
