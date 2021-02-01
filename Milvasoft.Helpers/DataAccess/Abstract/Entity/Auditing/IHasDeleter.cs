using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing
{
    /// <summary>
    /// Determines entity has deleter.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IHasDeleter<TKey> where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Deleter of entity.
        /// </summary>er
        TKey? DeleterUserId { get; set; }
    }
}
