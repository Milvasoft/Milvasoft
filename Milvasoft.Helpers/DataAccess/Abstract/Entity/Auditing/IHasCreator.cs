using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing
{
    /// <summary>
    /// Determines entity has creator.
    /// </summary>
    /// <typeparam name="TUserKey"></typeparam>
    public interface IHasCreator<TUserKey> where TUserKey : struct, IEquatable<TUserKey>
    {
        /// <summary>
        /// Creator of entity.
        /// </summary>
        TUserKey? CreatorUserId { get; set; }
    }
}
