using Microsoft.AspNetCore.Identity;
using Milvasoft.Helpers.DataAccess.Abstract.Entity.Auditing;
using System;

namespace Milvasoft.Helpers.DataAccess.Abstract.Entity
{
    /// <summary>
    /// Determines entity is fully auditable with user information.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IFullAuditable<TKey> : IAuditable<TKey>, IHasDeletionDate, IHasDeleter<TKey> where TKey : struct, IEquatable<TKey>
    {
    }

    /// <summary>
    /// Adds navigation properties to <see cref="IFullAuditable{TKey}"/> interface for user.
    /// </summary>
    /// <typeparam name="TKey">Key of the user</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    public interface IFullAuditable<TUser, TKey> : IAuditable<TUser, TKey>, IFullAuditable<TKey>
        where TUser : IdentityUser<TKey>
        where TKey : struct, IEquatable<TKey>
    {
    }

    /// <summary>
    /// Adds navigation properties to <see cref="IFullAuditable{TKey}"/> interface for user.
    /// </summary>
    /// <typeparam name="TKey">Key of the user</typeparam>
    /// <typeparam name="TUser">Type of the user</typeparam>
    public interface IFullAuditableWithCustomUser<TUser, TKey> : IFullAuditable<TKey>, IAuditableWithCustomUser<TUser, TKey>
        where TUser : IBaseEntity<TKey>
        where TKey : struct, IEquatable<TKey>
    {
    }

    /// <summary>
    /// Determines entity is fully auditable.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IFullAuditableWithoutUser<TKey> : IAuditableWithoutUser<TKey>, IHasDeletionDate
    {
    }
}
