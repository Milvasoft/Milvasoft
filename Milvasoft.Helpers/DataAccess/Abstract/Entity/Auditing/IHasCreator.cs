using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing
{
    /// <summary>
    /// Determines entity has creator.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IHasCreator<TKey> where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Creator of entity.
        /// </summary>
        TKey? CreatorUserId { get; set; }
    }
}
