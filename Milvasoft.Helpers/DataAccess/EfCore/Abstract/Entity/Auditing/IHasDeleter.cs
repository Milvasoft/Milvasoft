using System;

namespace Milvasoft.Helpers.DataAccess.EfCore.Abstract.Entity.Auditing;

/// <summary>
/// Determines entity has deleter.
/// </summary>
/// <typeparam name="TUserKey"></typeparam>
public interface IHasDeleter<TUserKey> where TUserKey : struct, IEquatable<TUserKey>
{
    /// <summary>
    /// Deleter of entity.
    /// </summary>er
    TUserKey? DeleterUserId { get; set; }
}

/// <summary>
/// Determines entity has deleter.
/// </summary>
public interface IHasDeleter
{
    /// <summary>
    /// Deleter of entity.
    /// </summary>er
    string DeleterUserId { get; set; }
}
