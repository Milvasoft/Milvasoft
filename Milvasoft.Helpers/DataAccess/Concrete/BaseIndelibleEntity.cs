using Milvasoft.Helpers.DataAccess.Abstract;
using System;

namespace Milvasoft.Helpers.DataAccess.Concrete
{
    /// <summary>
    /// Base entity for all of indelible entities. All indelible entities have this values.
    /// </summary>
    public abstract class BaseIndelibleEntity<TKey> : BaseEntity<TKey>, IBaseIndelibleEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Delete date in database for entity.
        /// </summary>
        public DateTime? DeletedDate { get; set; }

        /// <summary>
        /// State of entity in the database. Default value is true.
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
