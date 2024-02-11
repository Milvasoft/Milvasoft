using Milvasoft.Core;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Utils.Constants;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.Attributes.Validation;

/// <summary>
/// Determines minimum decimal value.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidateDecimalAttribute : ValidationAttribute
{
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

    /// <summary>
    /// Gets or sets error message localization flag.
    /// </summary>
    public bool LocalizeErrorMessages { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor of atrribute.
    /// </summary>
    public ValidateDecimalAttribute() { }

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
    /// <param name="maxValue"></param>
    public ValidateDecimalAttribute(decimal minValue, decimal maxValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
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
            IMilvaLocalizer milvaLocalizer;
            string localizedPropName;
            string errorMessage;

            if (LocalizeErrorMessages)
            {
                milvaLocalizer = context.GetMilvaLocalizer();

                localizedPropName = milvaLocalizer[LocalizerKey ?? $"{LocalizerKeys.Localized}{context.MemberName}"];
                errorMessage = FullMessage ? milvaLocalizer[LocalizerKey] : milvaLocalizer[LocalizerKeys.MinDecimalValueException, localizedPropName];
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
