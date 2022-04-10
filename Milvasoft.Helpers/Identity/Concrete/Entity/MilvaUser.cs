using System;

namespace Milvasoft.Helpers.Identity.Concrete.Entity;

/// <summary>
/// Represents a user in the identity system
/// </summary>
public class MilvaUser<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Gets or sets the primary key for this user.
    /// </summary>
    public virtual TKey Id { get; set; }

    /// <summary>
    /// Gets or sets the user name for this user.
    /// </summary>
    public virtual string UserName { get; set; }

    /// <summary>
    /// Gets or sets the normalized user name for this user.
    /// </summary>
    public virtual string NormalizedUserName { get; set; }

    /// <summary>
    /// Gets or sets the email address for this user.
    /// </summary>
    public virtual string Email { get; set; }

    /// <summary>
    /// Gets or sets the normalized email address for this user.
    /// </summary>
    public virtual string NormalizedEmail { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating if a user has confirmed their email address.
    /// </summary>
    /// <value>True if the email address has been confirmed, otherwise false.</value>
    public virtual bool EmailConfirmed { get; set; }

    /// <summary>
    /// Gets or sets a salted and hashed representation of the password for this user.
    /// </summary>
    public virtual string PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets a telephone number for the user.
    /// </summary>
    public virtual string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating if a user has confirmed their telephone address.
    /// </summary>
    /// <value>True if the telephone number has been confirmed, otherwise false.</value>
    public virtual bool PhoneNumberConfirmed { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating if two factor authentication is enabled for this user.
    /// </summary>
    /// <value>True if 2fa is enabled, otherwise false.</value>
    public virtual bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// Gets or sets the date and time, in UTC, when any user lockout ends.
    /// </summary>
    /// <remarks>
    /// A value in the past means the user is not locked out.
    /// </remarks>
    public virtual DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating if the user could be locked out.
    /// </summary>
    /// <value>True if the user could be locked out, otherwise false.</value>
    public virtual bool LockoutEnabled { get; set; }

    /// <summary>
    /// Gets or sets the number of failed login attempts for the current user.
    /// </summary>
    public virtual int AccessFailedCount { get; set; }
}
