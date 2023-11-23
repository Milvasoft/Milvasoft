using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.Utils.Constants;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.Attributes.Validation;

/// <summary>
/// Specifies that the class or property that this attribute is applied to requires the specified the valid id.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidateIdPropertyAttribute : ValidationAttribute
{

    #region Properties

    /// <summary>
    /// Gets or sets member name localizer key.
    /// </summary>
    public string MemberNameLocalizerKey { get; set; }

    /// <summary>
    /// If this is true, validation method won't check list is null.
    /// </summary>
    public bool DontCheckNullable { get; set; }

    /// <summary>
    /// Gets or sets error message localization flag.
    /// </summary>
    public bool LocalizeErrorMessages { get; set; }

    #endregion

    /// <summary>
    /// Constructor of <see cref="ValidateIdPropertyAttribute"/>.
    /// </summary>
    public ValidateIdPropertyAttribute() { }

    /// <summary>
    /// Determines whether the specified value of the object is valid.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        if (value != null)
        {
            IMilvaLocalizer milvaLocalizer;
            string localizedRelationName;
            string errorMessage;

            if (LocalizeErrorMessages)
            {
                milvaLocalizer = context.GetMilvaLocalizer();

                localizedRelationName = milvaLocalizer[MemberNameLocalizerKey ?? $"{LocalizerKeys.LocalizedEntityName}{context.MemberName[0..^2]}"].ToString().ToLowerInvariant();
                errorMessage = milvaLocalizer[LocalizerKeys.ValidationIdPropertyError, localizedRelationName];
            }
            else errorMessage = $"{LocalizerKeys.PleaseEnterAValid} {context.MemberName}";

            var httpContext = context.GetService<IHttpContextAccessor>().HttpContext;

            var valueType = value.GetType();

            if (valueType == typeof(Guid))
            {
                var guidIdString = value.ToString();

                var isParsed = Guid.TryParse(guidIdString, out Guid guidParameter);

                var regexMatchResult = guidParameter.ToString().MatchRegex("^[{]?[0-9a-fA-F]{8}"
                                                                          + "-([0-9a-fA-F]{4}-)"
                                                                          + "{3}[0-9a-fA-F]{12}[}]?$");

                if (!isParsed || guidParameter == default || guidParameter == Guid.Empty || !regexMatchResult)
                {
                    ErrorMessage = errorMessage;
                    httpContext.Items[context.MemberName] = ErrorMessage;
                    return new ValidationResult(FormatErrorMessage(""));
                }
            }
            else if (valueType == typeof(Guid?))
            {
                var guidParameter = (Guid?)value;

                var regexMatchResult = guidParameter.ToString().MatchRegex("^[{]?[0-9a-fA-F]{8}"
                                                                          + "-([0-9a-fA-F]{4}-)"
                                                                          + "{3}[0-9a-fA-F]{12}[}]?$");

                if (guidParameter == default || guidParameter == Guid.Empty || !regexMatchResult)
                {
                    ErrorMessage = errorMessage;
                    httpContext.Items[context.MemberName] = ErrorMessage;
                    return new ValidationResult(FormatErrorMessage(""));
                }
            }
            else if (valueType == typeof(int))
            {
                var intParameter = (int)value;

                if (intParameter <= 0)
                {
                    ErrorMessage = errorMessage;
                    httpContext.Items[context.MemberName] = ErrorMessage;
                    return new ValidationResult(FormatErrorMessage(""));
                }
            }
            else if (valueType == typeof(int?))
            {
                var intParameter = (int?)value;

                if (intParameter <= 0)
                {
                    ErrorMessage = errorMessage;
                    httpContext.Items[context.MemberName] = ErrorMessage;
                    return new ValidationResult(FormatErrorMessage(""));
                }
            }
            else if (valueType.GetGenericArguments()?.FirstOrDefault() == typeof(Guid))
            {
                var guidParameters = (List<Guid>)value;

                if (!DontCheckNullable && guidParameters.IsNullOrEmpty())
                {
                    ErrorMessage = errorMessage;
                    httpContext.Items[context.MemberName] = ErrorMessage;
                    return new ValidationResult(FormatErrorMessage(""));
                }

                if (!guidParameters.IsNullOrEmpty())
                    foreach (var guidParameter in guidParameters)
                    {
                        var regexMatchResult = guidParameter.ToString().MatchRegex("^[{]?[0-9a-fA-F]{8}"
                                                                                  + "-([0-9a-fA-F]{4}-)"
                                                                                  + "{3}[0-9a-fA-F]{12}[}]?$");

                        if (guidParameter == default || guidParameter == Guid.Empty || !regexMatchResult)
                        {
                            ErrorMessage = errorMessage;
                            httpContext.Items[context.MemberName] = ErrorMessage;
                            return new ValidationResult(FormatErrorMessage(""));
                        }
                    }
            }
            else if (valueType.GetGenericArguments()?.FirstOrDefault() == typeof(int))
            {
                var intParameters = (List<int>)value;

                if (!DontCheckNullable && intParameters.IsNullOrEmpty())
                {
                    ErrorMessage = errorMessage;
                    httpContext.Items[context.MemberName] = ErrorMessage;
                    return new ValidationResult(FormatErrorMessage(""));
                }

                if (!intParameters.IsNullOrEmpty())
                    foreach (var intParameter in intParameters)
                    {
                        if (intParameter <= 0)
                        {
                            ErrorMessage = errorMessage;
                            httpContext.Items[context.MemberName] = ErrorMessage;
                            return new ValidationResult(FormatErrorMessage(""));
                        }
                    }
            }
        }

        return ValidationResult.Success;
    }
}
