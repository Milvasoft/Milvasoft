using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.DependencyInjection;
using Milvasoft.Helpers.Enums;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Milvasoft.Helpers.Attributes.Validation
{
    /// <summary>
    /// Specifies that the class or property that this attribute is applied to requires the specified prevent string injection attacks and min/max length checks.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PreventListStringInjectionAttribute : ValidationAttribute
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
        /// Dummy class type for resource location.
        /// </summary>
        public Type ResourceType { get; set; } = null;

        /// <summary>
        /// If injection attack exist. Validation method will be send mail. 
        /// If has value mail will be send.
        /// </summary>
        public string MailContent { get; set; }


        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        /// <param name="localizedPropertyName"></param>
        public PreventListStringInjectionAttribute(int maximumLength, string localizedPropertyName)
        {
            MaximumLength = maximumLength;
            MemberNameLocalizerKey = localizedPropertyName;
        }
        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        public PreventListStringInjectionAttribute(int maximumLength)
        {
            MaximumLength = maximumLength;
        }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="minimumLength">The minimum length, inclusive.  It may not be negative.</param>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        /// <param name="localizedPropertyName"></param>
        public PreventListStringInjectionAttribute(int minimumLength, int maximumLength, string localizedPropertyName)
        {
            MaximumLength = maximumLength;
            MinimumLength = minimumLength;
            MemberNameLocalizerKey = localizedPropertyName;
        }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="minimumLength">The minimum length, inclusive.  It may not be negative.</param>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        public PreventListStringInjectionAttribute(int minimumLength, int maximumLength)
        {
            MaximumLength = maximumLength;
            MinimumLength = minimumLength;
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
            if (value != null)
            {
                var httpContext = context.GetService<IHttpContextAccessor>().HttpContext;

                IStringLocalizer sharedLocalizer = null;
                string localizedPropName;
                if (ResourceType != null)
                {
                    var localizerFactory = context.GetService<IStringLocalizerFactory>();

                    var assemblyName = new AssemblyName(ResourceType.GetTypeInfo().Assembly.FullName);
                    sharedLocalizer = localizerFactory.Create("SharedResource", assemblyName.Name);
                    localizedPropName = sharedLocalizer[MemberNameLocalizerKey ?? $"Localized{context.MemberName}"];
                }
                else localizedPropName = context.MemberName;



                // Check the lengths for legality
                CommonHelper.EnsureLegalLengths(MaximumLength, MinimumLength, sharedLocalizer);

                var stringList = ((List<string>)value);

                foreach (var stringValue in stringList)
                {
                    // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
                    // We expect a cast exception if a non-string was passed in.
                    int length = value == null ? 0 : stringValue.Length;
                    var lengthResult = value == null || (length >= this.MinimumLength && length <= this.MaximumLength);

                    if (!lengthResult)
                    {
                        ErrorMessage = sharedLocalizer != null
                             ? sharedLocalizer["PreventStringInjectionLengthResultNotTrue", localizedPropName, MinimumLength, MaximumLength]
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
                                    if (stringValue.Contains(invalidStringValue))
                                    {
                                        var milvasoftLogger = context.GetService<IMilvasoftLogger>();

                                        if (!string.IsNullOrEmpty(MailContent) && milvasoftLogger != null)
                                            milvasoftLogger.LogFatal(MailContent, MailSubject.Hack);

                                        ErrorMessage = sharedLocalizer != null
                                                       ? sharedLocalizer["PreventStringInjectionContainsForbiddenWordError", localizedPropName]
                                                       : $"{localizedPropName} contains invalid words.";

                                        return new ValidationResult(FormatErrorMessage(""));
                                    }
                    }
                    if (MinimumLength > 0 && (stringValue?.Length ?? 0) < MinimumLength)
                    {
                        ErrorMessage = sharedLocalizer != null
                                ? sharedLocalizer["PreventStringInjectionBellowMin", localizedPropName, MinimumLength]
                                : $"{localizedPropName} is below the minimum character limit. Please enter at least {MinimumLength} characters.";

                        httpContext.Items[context.MemberName] = ErrorMessage;

                        return new ValidationResult(FormatErrorMessage(""));
                    }

                }
            }

            return ValidationResult.Success;
        }

    }
}
