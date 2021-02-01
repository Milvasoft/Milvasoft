using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity
{
    /// <summary>
    /// Defines interface for base entity type. All entities in the system must implement this interface.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IBaseEntity<TKey> : IEntityBase<TKey> where TKey : struct, IEquatable<TKey>
    {
    }

    /// <summary>
    /// Defines interface for base entity type. All entities in the system must implement this interface.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntityBase<TKey>
    {
        /// <summary>
        /// Unique identifier for this entity.
        /// </summary>
        public TKey Id { get; set; }
    }
}
