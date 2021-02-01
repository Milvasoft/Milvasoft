using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing
{
    /// <summary>
    /// Determines entity has modifier.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IHasModifier<TKey> where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Last modifier of entity.
        /// </summary>
        TKey? LastModifierUserId { get; set; }
    }
}
