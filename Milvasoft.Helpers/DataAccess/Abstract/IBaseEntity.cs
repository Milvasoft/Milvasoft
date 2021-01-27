using System;

namespace Milvasoft.Helpers.DataAccess.Abstract
{
    /// <summary>
    /// Base interface for abstraction. 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IBaseEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Primary Key for entity. 
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        /// Date added to database for entity. 
        /// </summary>
        public DateTime InsertedDate { get; set; }

        /// <summary>
        /// Update date in database for entity. 
        /// </summary>
        public DateTime? LastUpdatedDate { get; set; }
    }
}
