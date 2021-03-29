using Milvasoft.Helpers.DataAccess.Abstract.Entity;
using System;

namespace Milvasoft.Helpers.MultiTenancy.EntityBase
{
    /// <summary>
    /// Tenant base.
    /// </summary>
    public interface IMilvaTenantBase<TKey> : IBaseEntity<TKey> where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Display name of the Tenant.
        /// </summary>
        string TenancyName { get; set; }

        /// <summary>
        /// Display name of the Tenant.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// ENCRYPTED connection string of the tenant database.
        /// Can be null if this tenant is stored in host database.
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Is this tenant active?
        /// If as tenant is not active, no user of this tenant can use the application.
        /// </summary>
        bool IsActive { get; set; }
    }

    /// <summary>
    /// Tenant base.
    /// </summary>
    public interface IMilvaBaseTenant<TKey> : IMilvaTenantBase<TKey>, IFullAuditable<TKey> where TKey : struct, IEquatable<TKey>
    {
    }
}
