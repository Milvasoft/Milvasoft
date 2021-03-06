﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.Helpers.Attributes.Validation
{
    /// <summary>
    /// Specifies that the class or property that this attribute is applied to requires the specified the valid id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidateIdPropertyAttribute : ValidationAttribute
    {

        #region Fields

        private readonly Type _resourceType = null;

        #endregion

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

        #endregion

        /// <summary>
        /// Constructor of <see cref="ValidateIdPropertyAttribute"/>.
        /// </summary>
        public ValidateIdPropertyAttribute() { }

        /// <summary>
        /// Constructor of <see cref="ValidateIdPropertyAttribute"/> for localization.
        /// </summary>
        /// <param name="resourceType"></param>
        public ValidateIdPropertyAttribute(Type resourceType)
        {
            _resourceType = resourceType;
        }

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
                IStringLocalizer sharedLocalizer;
                string localizedRelationName;
                string errorMessage;

                if (_resourceType != null)
                {
                    sharedLocalizer = context.GetLocalizerInstance(_resourceType);

                    localizedRelationName = sharedLocalizer[MemberNameLocalizerKey ?? $"{LocalizerKeys.LocalizedEntityName}{context.MemberName.Substring(0, context.MemberName.Length - 2)}"].ToString().ToLowerInvariant();
                    errorMessage = sharedLocalizer[LocalizerKeys.ValidationIdPropertyError, localizedRelationName];
                }
                else errorMessage = $"{LocalizerKeys.PleaseEnterAValid} {context.MemberName}";

                var httpContext = context.GetService<IHttpContextAccessor>().HttpContext;

                var valueType = value.GetType();

                if (valueType == typeof(Guid))
                {
                    var guidParameter = (Guid)value;

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
                else if (valueType.GetGenericArguments()[0] == typeof(Guid))
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
                else if (valueType.GetGenericArguments()[0] == typeof(int))
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
}
