using System;

namespace Milvasoft.Helpers.DataAccess.Abstract
{
    /// <summary>
    /// <para>  Base interface for abstraction. </para>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IBaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// <para> Primary Key for entity. </para>
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        /// <para> Date added to database for entity. </para>
        /// </summary>
        public DateTime InsertedDate { get; set; }

        /// <summary>
        /// <para> Update date in database for entity. </para>
        /// </summary>
        public DateTime? LastUpdatedDate { get; set; }
    }
}
