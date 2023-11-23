using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Utils.Constants;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.Attributes.Validation;

/// <summary>
/// Specifies that the class or property that this attribute is applied to requires the specified the valid id.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidateListCountAttribute : ValidationAttribute
{
    /// <summary>
    /// Gets or sets member name localizer key.
    /// </summary>
    public string LocalizerKey { get; set; }

    /// <summary>
    /// Minimum decimal value of requested validate scope.
    /// </summary>
    public int MinCount { get; } = -1;

    /// <summary>
    /// Maximum decimal value of requested validate scope.
    /// </summary>
    public int? MaxCount { get; set; }

    /// <summary>
    /// Gets or sets error message localization flag.
    /// </summary>
    public bool LocalizeErrorMessages { get; set; }

    #region Consructors

    /// <summary>
    /// Initializes new instance of <see cref="ValidateListCountAttribute"/>.
    /// </summary>
    public ValidateListCountAttribute()
    {
    }

    /// <summary>
    /// Initializes new instance of <see cref="ValidateListCountAttribute"/>.
    /// </summary>
    /// <param name="minCount"></param>
    public ValidateListCountAttribute(int minCount)
    {
        MinCount = minCount;
    }

    /// <summary>
    /// Initializes new instance of <see cref="ValidateListCountAttribute"/>.
    /// </summary>
    /// <param name="minCount"></param>
    /// <param name="maxCount"></param>
    public ValidateListCountAttribute(int minCount, int maxCount)
    {
        MinCount = minCount;
        MaxCount = maxCount;
    }

    #endregion

    /// <summary>
    /// Determines whether the specified value of the object is valid.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        IMilvaLocalizer milvaLocalizer = null;
        string localizedPropName = context.MemberName;

        if (LocalizeErrorMessages)
        {
            milvaLocalizer = context.GetMilvaLocalizer();

            localizedPropName = milvaLocalizer[LocalizerKey ?? $"{LocalizerKeys.Localized}{context.MemberName}"].ToString();
        }

        var httpContext = context.GetService<IHttpContextAccessor>().HttpContext;

        if (value != null)
        {
            var count = (int)value.GetType().GetProperty("Count").GetValue(value);

            if (MaxCount < 0)
                throw new MilvaValidationException(milvaLocalizer != null
                                                   ? milvaLocalizer[LocalizerKeys.ListMaxCountMessage]
                                                   : "Please enter a valid value for the maximum count.");

            if (MinCount < 0)
                throw new MilvaValidationException(milvaLocalizer != null
                                                   ? milvaLocalizer[LocalizerKeys.ListMinCountMessage]
                                                   : "Please enter a valid value for the minimum count.");

            if (MaxCount < MinCount)
                throw new MilvaValidationException(milvaLocalizer != null
                                                   ? milvaLocalizer[LocalizerKeys.PreventStringInjectionMinLengthBigThanMaxLengthException, MinCount, MaxCount]
                                                   : $"The minimum value ({MinCount}) you entered is greater than the maximum value ({MaxCount}). Please enter a valid range of values.");

            if (MaxCount.HasValue)
            {
                var countResult = count >= MinCount && count <= MaxCount;

                if (!countResult)
                {
                    ErrorMessage = milvaLocalizer != null
                                   ? MaxCount == MinCount
                                            ? milvaLocalizer[LocalizerKeys.ListCountMustBe, localizedPropName, MinCount]
                                            : milvaLocalizer[LocalizerKeys.ListCountNotValid, localizedPropName, MinCount, MaxCount]
                                   : MaxCount == MinCount
                                            ? $"The number of {localizedPropName} must be {MinCount}."
                                            : $"The number of {localizedPropName} must be minimum {MinCount} and maximum {MaxCount}.";

                    httpContext.Items[context.MemberName] = ErrorMessage;

                    return new ValidationResult(FormatErrorMessage(""));
                }
            }
            else if (MinCount > 0 && count < MinCount)
            {
                ErrorMessage = milvaLocalizer != null
                               ? milvaLocalizer[LocalizerKeys.ListCountBelowMin, MinCount, localizedPropName]
                               : $"Please select at least {MinCount} {localizedPropName}.";

                httpContext.Items[context.MemberName] = ErrorMessage;

                return new ValidationResult(FormatErrorMessage(""));
            }
        }
        else
        {
            if (MinCount > 0)
            {
                ErrorMessage = milvaLocalizer != null
                               ? milvaLocalizer[LocalizerKeys.ListCountBelowMin, MinCount, localizedPropName]
                               : $"Please select at least {MinCount} {localizedPropName}.";

                httpContext.Items[context.MemberName] = ErrorMessage;

                return new ValidationResult(FormatErrorMessage(""));
            }
        }

        return ValidationResult.Success;
    }
}
