using Microsoft.Extensions.Localization;
using Milvasoft.Core;
using Milvasoft.Core.Utils.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.Attributes.Validation;

/// <summary>
/// Determines minimum decimal value.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidateDecimalAttribute : ValidationAttribute
{

    #region Fields

    private readonly Type _resourceType = null;

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
    /// Minimum decimal value of requested validate scope.
    /// </summary>
    public decimal MinValue { get; } = -1;

    /// <summary>
    /// Maximum decimal value of requested validate scope.
    /// </summary>
    public decimal? MaxValue { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor of atrribute.
    /// </summary>
    public ValidateDecimalAttribute() { }

    /// <summary>
    /// Constructor of atrribute.
    /// </summary>
    /// <param name="resourceType"></param>
    public ValidateDecimalAttribute(Type resourceType)
    {
        _resourceType = resourceType;
    }

    /// <summary>
    /// Constructor of atrribute.
    /// </summary>
    /// <param name="minValue"></param>
    public ValidateDecimalAttribute(decimal minValue)
    {
        MinValue = minValue;
    }

    /// <summary>
    /// Constructor of atrribute.
    /// </summary>
    /// <param name="minValue"></param>
    /// <param name="resourceType"></param>
    public ValidateDecimalAttribute(decimal minValue, Type resourceType)
    {
        MinValue = minValue;
        _resourceType = resourceType;
    }

    /// <summary>
    /// Constructor of atrribute.
    /// </summary>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <param name="resourceType"></param>
    public ValidateDecimalAttribute(decimal minValue, decimal maxValue, Type resourceType = null)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        _resourceType = resourceType;
    }

    #endregion

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

                localizedPropName = sharedLocalizer[LocalizerKey ?? $"{LocalizerKeys.Localized}{context.MemberName}"];
                errorMessage = FullMessage ? sharedLocalizer[LocalizerKey] : sharedLocalizer[LocalizerKeys.MinDecimalValueException, localizedPropName];
            }
            else errorMessage = $"{LocalizerKeys.PleaseEnterAValid} {context.MemberName}.";

            if (decimal.TryParse(value.ToString(), out decimal decimalValue) && decimalValue <= MinValue)
            {
                ErrorMessage = errorMessage;
                return new ValidationResult(FormatErrorMessage(""));
            }
            if (MaxValue != null && decimalValue >= MaxValue)
            {
                ErrorMessage = errorMessage;
                return new ValidationResult(FormatErrorMessage(""));
            }
        }

        return ValidationResult.Success;
    }
}
