using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers;
using Milvasoft.Helpers.Attributes.Validation;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Utils;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpsiyonManagement.API.Helpers.Attributes.ValidationAttributes
{
    /// <summary>
    /// Specifies that the class or property that this attribute is applied to requires the specified the valid id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidateListCountAttribute : ValidationAttribute
    {
        #region Fields

        private readonly Type _resourceType = null;

        #endregion

        /// <summary>
        /// Gets or sets member name localizer key.
        /// </summary>
        public string LocalizerKey { get; set; }

        /// <summary>
        /// Minimum decimal value of requested validate scope.
        /// </summary>
        public int MinCount { get; } = -1;

        /// <summary>
        /// Maximum decimal value of requested validate scope.
        /// </summary>
        public int? MaxCount { get; set; }

        #region Consructors

        /// <summary>
        /// Initializes new instance of <see cref="ValidateListCountAttribute"/>.
        /// </summary>
        /// <param name="resourceType"></param>
        public ValidateListCountAttribute(Type resourceType)
        {
            _resourceType = resourceType;
        }

        /// <summary>
        /// Initializes new instance of <see cref="ValidateListCountAttribute"/>.
        /// </summary>
        /// <param name="minCount"></param>
        public ValidateListCountAttribute(int minCount)
        {
            MinCount = minCount;
        }

        /// <summary>
        /// Initializes new instance of <see cref="ValidateListCountAttribute"/>.
        /// </summary>
        /// <param name="minCount"></param>
        /// <param name="resourceType"></param>
        public ValidateListCountAttribute(int minCount, Type resourceType)
        {
            MinCount = minCount;
            _resourceType = resourceType;
        }

        /// <summary>
        /// Initializes new instance of <see cref="ValidateListCountAttribute"/>.
        /// </summary>
        /// <param name="minCount"></param>
        /// <param name="maxCount"></param>
        /// <param name="resourceType"></param>
        public ValidateListCountAttribute(int minCount, int maxCount, Type resourceType = null)
        {
            MinCount = minCount;
            MaxCount = maxCount;
            _resourceType = resourceType;
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
            IStringLocalizer sharedLocalizer = null;
            string localizedPropName = context.MemberName;

            if (_resourceType != null)
            {
                sharedLocalizer = context.GetLocalizerInstance(_resourceType);

                localizedPropName = sharedLocalizer[LocalizerKey ?? $"{LocalizerKeys.LocalizedEntityName}{context.MemberName}"].ToString();
            }

            var httpContext = context.GetService<IHttpContextAccessor>().HttpContext;

            if (value != null)
            {
                var count = (int)value.GetType().GetProperty("Count").GetValue(value);

                if (MaxCount < 0)
                    throw new MilvaValidationException(sharedLocalizer != null
                                                       ? sharedLocalizer[LocalizerKeys.ListMaxCountMessage]
                                                       : "Please enter a valid value for the maximum count.");

                if (MinCount < 0)
                    throw new MilvaValidationException(sharedLocalizer != null
                                                       ? sharedLocalizer[LocalizerKeys.ListMinCountMessage]
                                                       : "Please enter a valid value for the minimum count.");

                if (MaxCount < MinCount)
                    throw new MilvaValidationException(sharedLocalizer != null
                                                       ? sharedLocalizer[LocalizerKeys.PreventStringInjectionMinLengthBigThanMaxLengthException, MinCount, MaxCount]
                                                       : $"The minimum value ({MinCount}) you entered is greater than the maximum value ({MaxCount}). Please enter a valid range of values.");

                if (MaxCount.HasValue)
                {
                    var countResult = count >= MinCount && count <= MaxCount;

                    if (!countResult)
                    {
                        ErrorMessage = sharedLocalizer != null
                                       ? MaxCount == MinCount
                                                ? sharedLocalizer[LocalizerKeys.ListCountMustBe, localizedPropName, MinCount]
                                                : sharedLocalizer[LocalizerKeys.ListCountNotValid, localizedPropName, MinCount, MaxCount]
                                       : MaxCount == MinCount
                                                ? $"The number of {localizedPropName} must be {MinCount}."
                                                : $"The number of {localizedPropName} must be minimum {MinCount} and maximum {MaxCount}.";

                        httpContext.Items[context.MemberName] = ErrorMessage;

                        return new ValidationResult(FormatErrorMessage(""));
                    }
                }
                else if (MinCount > 0 && count < MinCount)
                {
                    ErrorMessage = sharedLocalizer != null
                                   ? sharedLocalizer[LocalizerKeys.ListCountBelowMin, localizedPropName, MinCount]
                                   : $"Please select at least {MinCount} {localizedPropName}.";

                    httpContext.Items[context.MemberName] = ErrorMessage;

                    return new ValidationResult(FormatErrorMessage(""));
                }
            }
            else
            {
                if (MinCount > 0)
                {
                    ErrorMessage = sharedLocalizer != null
                                   ? sharedLocalizer[LocalizerKeys.ListCountBelowMin, localizedPropName, MinCount]
                                   : $"Please select at least {MinCount} {localizedPropName}.";

                    httpContext.Items[context.MemberName] = ErrorMessage;

                    return new ValidationResult(FormatErrorMessage(""));
                }
            }

            return ValidationResult.Success;
        }
    }
}
