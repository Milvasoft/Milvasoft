using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Milvasoft.Core.EntityBase.Abstract;
using Milvasoft.Core.Utils.Constants;

namespace Milvasoft.Identity.Concrete;

/// <summary>
/// Provides an abstraction for user validation.
/// </summary>
public class MilvaUserValidation<TUser, TKey, TLocalizer> : IUserValidator<TUser>
    where TUser : IdentityUser<TKey>, IEntityBase<TKey>
    where TKey : IEquatable<TKey>
    where TLocalizer : IStringLocalizer
{
    private readonly TLocalizer _localizer;

    /// <summary>
    /// Constructor for localizer dependenct injection.
    /// </summary>
    /// <param name="localizer"></param>
    public MilvaUserValidation(TLocalizer localizer) => _localizer = localizer;

    /// <summary>
    /// Validates the specified user as an asynchronous operation.
    /// </summary>
    public virtual Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    {
        List<IdentityError> errors = new();

        //Checking that the username does not start with a numeric expression
        if (int.TryParse(user.UserName[0].ToString(), out int _))
            errors.Add(new IdentityError { Code = "UserNameNumberStartWith", Description = _localizer[LocalizerKeys.UserValidationUserNameNumberStartWith] });

        //UserName is between 3 and 25 characters
        if (user.UserName.Length < 3 && user.UserName.Length > 25)
            errors.Add(new IdentityError { Code = "UserNameLength", Description = _localizer[LocalizerKeys.UserValidationUserNameLength] });

        //Control that the email does not exceed 70 characters
        if (user.Email?.Length > 70)
            errors.Add(new IdentityError { Code = "EmailLength", Description = _localizer[LocalizerKeys.UserValidationEmailLength] });

        if (!errors.Any())
            return Task.FromResult(IdentityResult.Success);

        return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
    }
}
