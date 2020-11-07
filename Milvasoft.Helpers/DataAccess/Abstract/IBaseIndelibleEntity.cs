using System;

namespace Milvasoft.Helpers.DataAccess.Abstract
{

    /// <summary>
    /// <para> Base entity for all of indelible entities. All indelible entities have this values.</para>
    /// </summary>
    public interface IBaseIndelibleEntity<TKey> : IBaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// <para> Gets or sets the DeletedDate. </para>
        /// </summary>
        public DateTime? DeletedDate { get; set; }

        /// <summary>
        /// <para> Gets or sets a value indicating whether IsDeleted.</para>
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
