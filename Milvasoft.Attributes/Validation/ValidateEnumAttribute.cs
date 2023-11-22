using Microsoft.Extensions.Localization;
using Milvasoft.Core;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Utils.Constants;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.Attributes.Validation;

/// <summary>
/// Validates value is defined in enum.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidateEnumAttribute : ValidationAttribute
{
    #region Fields

    private readonly Type _enumType = null;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the error message spesific content
    /// </summary>
    public string LocalizerKey { get; set; }

    /// <summary>
    /// Gets or sets the full error message spesific content.
    /// </summary>
    public bool FullMessage { get; set; }

    /// <summary>
    /// Gets or sets error message localization flag.
    /// </summary>
    public bool LocalizeErrorMessages { get; set; }

    #endregion

    /// <summary>
    /// Constructor of atrribute.
    /// </summary>
    public ValidateEnumAttribute()
    {
    }

    /// <summary>
    /// Constructor of atrribute.
    /// </summary>
    /// <param name="enumType"></param>
    public ValidateEnumAttribute(Type enumType)
    {
        _enumType = enumType;
    }

    /// <summary>
    /// Determines whether the specified value of the object is valid.
    /// </summary>
    /// 
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        if (value != null)
        {
            IMilvaLocalizer milvaLocalizer;
            string localizedPropName;
            string errorMessage;

            if (LocalizeErrorMessages)
            {
                milvaLocalizer = context.GetMilvaLocalizer();

                localizedPropName = milvaLocalizer[LocalizerKey ?? $"{LocalizerKeys.Localized}{context.MemberName}"];
                errorMessage = FullMessage ? milvaLocalizer[LocalizerKey] : milvaLocalizer[LocalizerKeys.PleaseSelectAValid, localizedPropName];
            }
            else errorMessage = $"{LocalizerKeys.PleaseEnterAValid} {context.MemberName}.";

            var valueType = _enumType ?? value.GetType();

            if (!Enum.IsDefined(valueType, value))
            {
                ErrorMessage = errorMessage;
                return new ValidationResult(FormatErrorMessage(""));
            }
        }

        return ValidationResult.Success;
    }
}
