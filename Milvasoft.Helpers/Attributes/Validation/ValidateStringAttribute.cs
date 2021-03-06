﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.Enums;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models;
using Milvasoft.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.Helpers.Attributes.Validation
{
    /// <summary>
    /// Specifies that the class or property that this attribute is applied to requires the specified prevent string injection attacks and min/max length checks.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidateStringAttribute : ValidationAttribute
    {
        #region Properties

        /// <summary>
        /// Gets or sets the error message spesific content
        /// </summary>
        public string MemberNameLocalizerKey { get; set; }

        /// <summary>
        /// Gets the maximum acceptable length of the string
        /// </summary>
        public int MaximumLength { get; private set; }

        /// <summary>
        /// Gets or sets the minimum acceptable length of the string
        /// </summary>
        public int MinimumLength { get; private set; } = 0;

        /// <summary>
        /// If injection attack exist. Validation method will be send mail. 
        /// If has value mail will be send.
        /// </summary>
        public string MailContent { get; set; }

        /// <summary>
        /// Dummy class type for resource location.
        /// </summary>
        public Type ResourceType { get; set; } = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        public ValidateStringAttribute(int maximumLength)
        {
            MaximumLength = maximumLength;
        }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="minimumLength">The minimum length, inclusive.  It may not be negative.</param>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        public ValidateStringAttribute(int minimumLength, int maximumLength)
        {
            MaximumLength = maximumLength;
            MinimumLength = minimumLength;
        }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        /// <param name="memberNameLocalizerKey"></param>
        public ValidateStringAttribute(int maximumLength, string memberNameLocalizerKey)
        {
            MaximumLength = maximumLength;
            MemberNameLocalizerKey = memberNameLocalizerKey;
        }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="minimumLength">The minimum length, inclusive.  It may not be negative.</param>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        /// <param name="memberNameLocalizerKey"></param>
        public ValidateStringAttribute(int minimumLength, int maximumLength, string memberNameLocalizerKey)
        {
            MaximumLength = maximumLength;
            MinimumLength = minimumLength;
            MemberNameLocalizerKey = memberNameLocalizerKey;
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
            if (value == null) value = "";

            var valueType = value.GetType();

            if (typeof(List<string>).IsAssignableFrom(valueType) || valueType.IsAssignableTo(typeof(List<string>)))
            {
                var stringList = value != null ? (List<string>)value : null;

                if (!stringList.IsNullOrEmpty())
                    foreach (var stringValue in stringList)
                        return GetValidationResult(stringValue, context);
            }
            else
            {
                var stringValue = value != null ? (string)value : null;

                return GetValidationResult(stringValue, context);
            }

            return ValidationResult.Success;

            ValidationResult GetValidationResult(string stringValue, ValidationContext context)
            {
                IStringLocalizer sharedLocalizer = null;
                string localizedPropName;
                if (ResourceType != null)
                {
                    sharedLocalizer = context.GetLocalizerInstance(ResourceType);

                    localizedPropName = sharedLocalizer[MemberNameLocalizerKey ?? $"{LocalizerKeys.Localized}{context.MemberName}"];
                }
                else localizedPropName = context.MemberName;

                var httpContext = context.GetService<IHttpContextAccessor>().HttpContext;

                // Check the lengths for legality
                CommonHelper.EnsureLegalLengths(MaximumLength, MinimumLength, sharedLocalizer);

                // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
                // We expect a cast exception if a non-string was passed in.
                int length = stringValue == null ? 0 : stringValue.Length;

                var lengthResult = stringValue == null || (length >= MinimumLength && length <= MaximumLength);

                if (!lengthResult)
                {
                    ErrorMessage = sharedLocalizer != null
                                   ? sharedLocalizer[LocalizerKeys.PreventStringInjectionLengthResultNotTrue, localizedPropName, MinimumLength, MaximumLength]
                                   : $"{localizedPropName} must have a character length in the range {MinimumLength} to {MaximumLength}.";

                    httpContext.Items[context.MemberName] = ErrorMessage;

                    return new ValidationResult(FormatErrorMessage(""));
                }

                if (!string.IsNullOrEmpty(stringValue))
                {
                    var blackList = context.GetService<List<InvalidString>>();

                    if (!blackList.IsNullOrEmpty())
                        foreach (var invalidString in blackList)
                            foreach (var invalidStringValue in invalidString.Values)
                                if (stringValue.ToLowerInvariant().Contains(invalidStringValue))
                                {
                                    var milvasoftLogger = context.GetService<IMilvaLogger>();

                                    if (!string.IsNullOrEmpty(MailContent) && milvasoftLogger != null)
                                        milvasoftLogger.LogFatal(MailContent, MailSubject.Hack);

                                    ErrorMessage = sharedLocalizer != null
                                                   ? sharedLocalizer[LocalizerKeys.PreventStringInjectionContainsForbiddenWordError, localizedPropName]
                                                   : $"{localizedPropName} contains invalid words.";

                                    return new ValidationResult(FormatErrorMessage(""));
                                }
                }

                if (MinimumLength > 0 && (stringValue?.Length ?? 0) < MinimumLength)
                {
                    ErrorMessage = sharedLocalizer != null
                                   ? sharedLocalizer[LocalizerKeys.PreventStringInjectionBellowMin, localizedPropName, MinimumLength]
                                   : $"{localizedPropName} is below the minimum character limit. Please enter at least {MinimumLength} characters.";

                    httpContext.Items[context.MemberName] = ErrorMessage;

                    return new ValidationResult(FormatErrorMessage(""));
                }

                return ValidationResult.Success;
            }

        }
    }
}
