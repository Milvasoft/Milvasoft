using System;

namespace Milvasoft.Helpers.DataAccess.Abstract
{

    /// <summary>
    /// Base entity for all of indelible entities. All indelible entities have this values.
    /// </summary>
    public interface IBaseIndelibleEntity<TKey> : IBaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets or sets the DeletedDate. 
        /// </summary>
        public DateTime? DeletedDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsDeleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
