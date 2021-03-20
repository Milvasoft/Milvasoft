using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing
{
    /// <summary>
    /// Determines entity has modifier.
    /// </summary>
    /// <typeparam name="TUserKey"></typeparam>
    public interface IHasModifier<TUserKey> where TUserKey :  IEquatable<TUserKey>
    {
        /// <summary>
        /// Last modifier of entity.
        /// </summary>
        TUserKey? LastModifierUserId { get; set; }
    }
}
