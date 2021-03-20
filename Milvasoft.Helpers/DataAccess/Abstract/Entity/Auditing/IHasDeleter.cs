using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing
{
    /// <summary>
    /// Determines entity has deleter.
    /// </summary>
    /// <typeparam name="TUserKey"></typeparam>
    public interface IHasDeleter<TUserKey> where TUserKey :  IEquatable<TUserKey>
    {
        /// <summary>
        /// Deleter of entity.
        /// </summary>er
        TUserKey? DeleterUserId { get; set; }
    }
}
