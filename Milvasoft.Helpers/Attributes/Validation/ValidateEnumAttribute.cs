using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.Helpers.Attributes.Validation;

/// <summary>
/// Validates value is defined in enum.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidateEnumAttribute : ValidationAttribute
{
    #region Fields

    private readonly Type _resourceType = null;
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

    #endregion

    /// <summary>
    /// Constructor of atrribute.
    /// </summary>
    /// <param name="resourceType"></param>
    /// <param name="enumType"></param>
    public ValidateEnumAttribute(Type resourceType, Type enumType)
    {
        _resourceType = resourceType;
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
            IStringLocalizer sharedLocalizer;
            string localizedPropName;
            string errorMessage;

            if (_resourceType != null)
            {
                sharedLocalizer = context.GetLocalizerInstance(_resourceType);

                localizedPropName = sharedLocalizer[LocalizerKey != null ? LocalizerKey : $"{LocalizerKeys.Localized}{context.MemberName}"];
                errorMessage = FullMessage ? sharedLocalizer[LocalizerKey] : sharedLocalizer[LocalizerKeys.PleaseEnterAValid, localizedPropName];
            }
            else errorMessage = $"{LocalizerKeys.PleaseEnterAValid} {context.MemberName}.";

            if (!Enum.IsDefined(_enumType, value))
            {
                ErrorMessage = errorMessage;
                return new ValidationResult(FormatErrorMessage(""));
            }
        }

        return ValidationResult.Success;
    }
}
