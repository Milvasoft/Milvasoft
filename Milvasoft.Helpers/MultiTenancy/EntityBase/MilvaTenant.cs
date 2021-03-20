using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing;
using System;

namespace Milvasoft.Helpers.MultiTenancy.EntityBase
{
    public abstract class MilvaTenant<TUser, TUserKey> : MilvaTenantBase<TenantId>, IFullAuditable<TUser, TUserKey, TenantId>
    where TUser : IdentityUser<TUserKey>
    where TUserKey :  IEquatable<TUserKey>
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
        /// Display name of the Tenant.
        /// </summary>
        public virtual int BranchNo
        {
            get => _branchNo;
            set => _branchNo = value;
        }

        public TUser DeleterUser { get; set; }
        public TUserKey? DeleterUserId { get; set; }
        public TUser LastModifierUser { get; set; }
        public TUserKey? LastModifierUserId { get; set; }
        public TUser CreatorUser { get; set; }
        public TUserKey? CreatorUserId { get; set; }

        /// <summary>
        /// Creates a new tenant.
        /// </summary>
        /// <param name="tenancyName">UNIQUE name of this Tenant</param>
        /// <param name="branchNo"></param>
        /// <param name="name">Display name of the Tenant</param>
        protected MilvaTenant(string tenancyName, int branchNo, string name)
        {
            _tenancyName = tenancyName;
            _branchNo = branchNo;
            Name = name;
            IsActive = true;
        }
    }
}
