using Milvasoft.Helpers.MultiTenancy.EntityBase;
using System;
using System.Collections.Generic;

namespace Milvasoft.SampleAPI.DTOs
{
    /// <summary>
    /// Cached tenant model for distributed redis server.
    /// </summary>
    public record CachedTenant : IMilvaTenantBase<TenantId>
    {
        private string _tenancyName;

        /// <summary>
        /// Id of tenant.
        /// </summary>
        public TenantId Id { get; set; }

        /// <summary>
        /// Name of tenant.
        /// </summary>
        public string TenancyName { get => Id.TenancyName; set => _tenancyName = value; }

        /// <summary>
        /// Branch number of tenant.
        /// </summary>
        public int BranchNo { get => Id.BranchNo; }

        /// <summary>
        /// Display name of tenant.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Database connection string of tenant.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Purchased table count of tenant.
        /// </summary>
        public int TableCount { get; set; }

        /// <summary>
        /// Subscription expiration date of tenant.
        /// </summary>
        public DateTime SubscriptionExpireDate { get; set; }

        /// <summary>
        /// Determines tenant is active or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Purchased modules of tenant.
        /// </summary>
        public List<string> Modules { get; set; }
    }
}
