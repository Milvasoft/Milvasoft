using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Milvasoft.Helpers.Attributes.Validation
{
    /// <summary>
    /// Specifies that the class or property that this attribute is applied to requires the specified must match the localized regex.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MilvaRegexAttribute : RegularExpressionAttribute
    {

        #region Fields

        private readonly Type _resourceType = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the error message spesific content
        /// </summary>
        public string MemberNameLocalizerKey { get; set; }

        /// <summary>
        /// Gets or sets localized examle regex format.
        /// </summary>
        public string ExampleFormatLocalizerKey { get; set; }

        /// <summary>
        /// Gets or sets property is required.
        /// </summary>
        public bool IsRequired { get; set; } = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        public MilvaRegexAttribute() : base(@"^()$") { }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        public MilvaRegexAttribute(Type resourceType) : base(@"^()$")
        {
            _resourceType = resourceType;
        }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="pattern"></param>
        public MilvaRegexAttribute(string pattern) : base(pattern) { }

        #endregion

        /// <summary>
        /// Determines whether the specified value of the object is valid.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var localizerFactory = context.GetService<IStringLocalizerFactory>();

            var assemblyName = new AssemblyName(_resourceType.GetTypeInfo().Assembly.FullName);
            var sharedLocalizer = localizerFactory.Create(_resourceType.Name, assemblyName.Name);

            var localizedPropName = sharedLocalizer == null ? context.MemberName : sharedLocalizer[$"{LocalizerKeys.Localized}{MemberNameLocalizerKey ?? context.MemberName}"];

            if (sharedLocalizer != null)
            {
                if (IsRequired)
                {
                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                    {
                        var localizedPattern = sharedLocalizer[$"{LocalizerKeys.RegexPattern}{MemberNameLocalizerKey ?? context.MemberName}"];

                        if (RegexMatcher.MatchRegex(value.ToString(), sharedLocalizer[localizedPattern]))
                            return ValidationResult.Success;
                        else
                        {
                            var exampleFormat = sharedLocalizer[ExampleFormatLocalizerKey ?? LocalizerKeys.RegexExample + context.MemberName];
                            ErrorMessage = sharedLocalizer[LocalizerKeys.RegexErrorMessage, localizedPropName, exampleFormat];
                            return new ValidationResult(FormatErrorMessage(""));
                        }
                    }
                    else
                    {
                        ErrorMessage = sharedLocalizer[LocalizerKeys.PropertyIsRequired, localizedPropName];
                        return new ValidationResult(FormatErrorMessage(""));
                    }
                }
                else
                {
                    if (value != null && !string.IsNullOrEmpty(value.ToString()))
                    {
                        var localizedPattern = sharedLocalizer[$"{LocalizerKeys.RegexPattern}{MemberNameLocalizerKey ?? context.MemberName}"];

                        if (RegexMatcher.MatchRegex(value.ToString(), sharedLocalizer[localizedPattern]))
                            return ValidationResult.Success;
                        else
                        {
                            var exampleFormat = sharedLocalizer[ExampleFormatLocalizerKey ?? LocalizerKeys.RegexExample + context.MemberName];
                            ErrorMessage = sharedLocalizer[LocalizerKeys.RegexErrorMessage, localizedPropName, exampleFormat];
                            return new ValidationResult(FormatErrorMessage(""));
                        }
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
            }
            else
            {
                if (base.IsValid(value)) return ValidationResult.Success;
                else
                {
                    ErrorMessage = $"Please enter a valid{context.MemberName}";
                    return new ValidationResult(FormatErrorMessage(""));
                }
            }

        }
    }
}