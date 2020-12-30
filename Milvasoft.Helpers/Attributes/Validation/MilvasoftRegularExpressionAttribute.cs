using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Milvasoft.Helpers.Attributes.Validation
{
    /// <summary>
    /// Specifies that the class or property that this attribute is applied to requires the specified must match the localized regex.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MilvasoftRegularExpressionAttribute : RegularExpressionAttribute
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

        /// <summary>
        /// Dummy class type for resource location.
        /// </summary>
        public Type ResourceType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        public MilvasoftRegularExpressionAttribute() : base(@"^()$") { }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        public MilvasoftRegularExpressionAttribute(Type resourceType) : base(@"^()$")
        {
            _resourceType = resourceType;
        }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="pattern"></param>
        public MilvasoftRegularExpressionAttribute(string pattern) : base(pattern) { }

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
            var sharedLocalizer = localizerFactory.Create("SharedResource", assemblyName.Name);

            var localizedPropName = sharedLocalizer[$"Localized{MemberNameLocalizerKey ?? context.MemberName}"];

            var httpContext = context.GetService<IHttpContextAccessor>().HttpContext;//TODO http contexte items eklenecek mi ?

            if (IsRequired)
            {
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    var localizedPattern = sharedLocalizer[$"RegexPattern{MemberNameLocalizerKey ?? context.MemberName}"];

                    if (RegexMatcher.MatchRegex(value.ToString(), sharedLocalizer[localizedPattern]))
                        return ValidationResult.Success;
                    else
                    {
                        var exampleFormat = sharedLocalizer[ExampleFormatLocalizerKey ?? $"RegexExample{context.MemberName}"];
                        ErrorMessage = sharedLocalizer["RegexErrorMessage", localizedPropName, exampleFormat];
                        return new ValidationResult(FormatErrorMessage(""));
                    }
                }
                else
                {
                    ErrorMessage = sharedLocalizer["PropertyIsRequired", localizedPropName];
                    return new ValidationResult(FormatErrorMessage(""));
                }
            }
            else
            {
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    var localizedPattern = sharedLocalizer[$"RegexPattern{MemberNameLocalizerKey ?? context.MemberName}"];

                    if (RegexMatcher.MatchRegex(value.ToString(), sharedLocalizer[localizedPattern]))
                        return ValidationResult.Success;
                    else
                    {
                        var exampleFormat = sharedLocalizer[ExampleFormatLocalizerKey ?? $"RegexExample{context.MemberName}"];
                        ErrorMessage = sharedLocalizer["RegexErrorMessage", localizedPropName, exampleFormat];
                        return new ValidationResult(FormatErrorMessage(""));
                    }
                }
                else
                {
                    return ValidationResult.Success;
                }
            }

        }
    }
}
