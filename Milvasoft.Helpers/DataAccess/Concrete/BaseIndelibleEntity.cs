using Milvasoft.Helpers.DataAccess.Abstract;
using System;

namespace Milvasoft.Helpers.DataAccess.Concrete
{
    /// <summary>
    /// <para> Base entity for all of indelible entities. All indelible entities have this values. </para>
    /// </summary>
    public abstract class BaseIndelibleEntity<TKey> : BaseEntity<TKey>, IBaseIndelibleEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// <para> Delete date in database for entity. </para>
        /// </summary>
        public DateTime? DeletedDate { get; set; }

        /// <summary>
        /// <para> State of entity in the database. Default value is true. </para>
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
