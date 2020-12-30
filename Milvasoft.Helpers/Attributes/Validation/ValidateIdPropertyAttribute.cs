using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Milvasoft.Helpers.Attributes.Validation
{
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
        /// Gets or sets error message localizer key.
        /// </summary>
        public string ErrorMessageLocalizerKey { get; set; }

        /// <summary>
        /// If this is true, validation method won't check list is null.
        /// </summary>
        public bool DontCheckNullable { get; set; }

        /// <summary>
        /// Dummy class type for resource location.
        /// </summary>
        public Type ResourceType { get; set; }

        #endregion


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
                var localizerFactory = context.GetRequiredService<IStringLocalizerFactory>();

                var assemblyName = new AssemblyName(ResourceType.GetTypeInfo().Assembly.FullName);
                var sharedLocalizer = localizerFactory.Create("SharedResource", assemblyName.Name);

                var localizedRelationName = sharedLocalizer[MemberNameLocalizerKey ?? $"LocalizedEntityName{context.MemberName.Substring(0, context.MemberName.Length - 2)}"].ToString().ToLowerInvariant();
                var propertyValue = value;

                var httpContext = context.GetRequiredService<IHttpContextAccessor>().HttpContext;

                var valueType = propertyValue.GetType();

                var errorMessage = sharedLocalizer["ValidationIdPropertyError", localizedRelationName];


                if (valueType == typeof(Guid))
                {
                    var guidParameter = (Guid)propertyValue;

                    var regexMatchResult = guidParameter.ToString().MatchRegex("^[{]?[0-9a-fA-F]{8}"
                                                                              + "-([0-9a-fA-F]{4}-)"
                                                                              + "{3}[0-9a-fA-F]{12}[}]?$");

                    if (guidParameter == default || guidParameter == Guid.Empty || !regexMatchResult)
                    {
                        ErrorMessage = sharedLocalizer["ValidationIdPropertyError", localizedRelationName];
                        httpContext.Items[context.MemberName] = ErrorMessage;
                        return new ValidationResult(FormatErrorMessage(""));
                    }
                }
                else if (valueType == typeof(Guid?))
                {
                    var guidParameter = (Guid?)propertyValue;

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
                    var intParameter = (int)propertyValue;

                    if (intParameter <= 0)
                    {
                        ErrorMessage = errorMessage;
                        httpContext.Items[context.MemberName] = ErrorMessage;
                        return new ValidationResult(FormatErrorMessage(""));
                    }
                }
                else if (valueType.GetGenericArguments()[0] == typeof(Guid))
                {
                    var guidParameters = (List<Guid>)propertyValue;

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
            }

            return ValidationResult.Success;
        }
    }
}
