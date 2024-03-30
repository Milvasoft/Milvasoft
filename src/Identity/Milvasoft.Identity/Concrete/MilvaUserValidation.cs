using Microsoft.AspNetCore.Identity;

namespace Milvasoft.Identity.Concrete;

/// <summary>
/// Provides an abstraction for user validation.
/// </summary>
/// <remarks>
/// Constructor for localizer dependenct injection.
/// </remarks>
/// <param name="localizer"></param>
public class MilvaUserValidation<TUser, TKey>(IMilvaLocalizer localizer) : IUserValidator<TUser>
    where TUser : IdentityUser<TKey>, IEntityBase<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly IMilvaLocalizer _localizer = localizer;

    /// <summary>
    /// Validates the specified user as an asynchronous operation.
    /// </summary>
    public virtual Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    {
        List<IdentityError> errors = [];

        //Checking that the username does not start with a numeric expression
        if (int.TryParse(user.UserName[0].ToString(), out int _))
            errors.Add(new IdentityError { Code = "UserNameNumberStartWith", Description = _localizer[LocalizerKeys.UserValidationUserNameNumberStartWith] });

        //UserName is between 3 and 25 characters
        if (user.UserName.Length < 3 && user.UserName.Length > 25)
            errors.Add(new IdentityError { Code = "UserNameLength", Description = _localizer[LocalizerKeys.UserValidationUserNameLength] });

        //Control that the email does not exceed 70 characters
        if (user.Email?.Length > 70)
            errors.Add(new IdentityError { Code = "EmailLength", Description = _localizer[LocalizerKeys.UserValidationEmailLength] });

        if (errors.Count == 0)
            return Task.FromResult(IdentityResult.Success);

#pragma warning disable S3878 // Arrays should not be created for params parameters
        return Task.FromResult(IdentityResult.Failed([.. errors]));
#pragma warning restore S3878 // Arrays should not be created for params parameters
    }
}
