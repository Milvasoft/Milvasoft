using System;

namespace Milvasoft.Helpers.DataAccess.EfCore.Abstract.Entity.Auditing;

/// <summary>
/// Determines entity has modifier.
/// </summary>
/// <typeparam name="TUserKey"></typeparam>
public interface IHasModifier<TUserKey> where TUserKey : struct, IEquatable<TUserKey>
{
    /// <summary>
    /// Last modifier of entity.
    /// </summary>
    TUserKey? LastModifierUserId { get; set; }
}

/// <summary>
/// Determines entity has modifier.
/// </summary>
public interface IHasModifier
{
    /// <summary>
    /// Last modifier of entity.
    /// </summary>
    string LastModifierUserId { get; set; }
}
