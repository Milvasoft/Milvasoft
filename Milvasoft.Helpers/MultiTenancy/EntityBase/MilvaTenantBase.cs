using Milvasoft.Helpers.DataAccess.Concrete.Entity;
using System;

namespace Milvasoft.Helpers.MultiTenancy.EntityBase
{
    /// <summary>
    /// Tenant base.
    /// </summary>
    public abstract class MilvaTenantBase<TKey> : FullAuditableEntity<TKey>, IMilvaTenantBase where TKey :  IEquatable<TKey>
    {
        /// <summary>
        /// Display name of the Tenant.
        /// </summary>
        public virtual string TenancyName { get; set; }

        /// <summary>
        /// Display name of the Tenant.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// ENCRYPTED connection string of the tenant database.
        /// Can be null if this tenant is stored in host database.
        /// </summary>
        public virtual string ConnectionString { get; set; }

        /// <summary>
        /// Is this tenant active?
        /// If as tenant is not active, no user of this tenant can use the application.
        /// </summary>
        public virtual bool IsActive { get; set; }
    }
}
