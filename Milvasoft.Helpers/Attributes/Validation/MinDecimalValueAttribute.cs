using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Milvasoft.Helpers.Attributes.Validation
{
    /// <summary>
    /// Determines minimum decimal value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MinDecimalValueAttribute : ValidationAttribute
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
        public int MinValue { get; } = -1;

        /// <summary>
        /// Dummy class type for resource location.
        /// </summary>
        public Type ResourceType { get; set; }

        #endregion


        #region Constructors

        /// <summary>
        /// Constructor of atrribute.
        /// </summary>
        public MinDecimalValueAttribute() { }

        /// <summary>
        /// Constructor of atrribute.
        /// </summary>
        /// <param name="minValue"></param>
        public MinDecimalValueAttribute(int minValue)
        {
            MinValue = minValue;
        }

        /// <summary>
        /// Constructor of atrribute.
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="localizedPropertyName"></param>
        public MinDecimalValueAttribute(int minValue, string localizedPropertyName)
        {
            MinValue = minValue;
            LocalizerKey = localizedPropertyName;
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
            var localizerFactory = context.GetRequiredService<IStringLocalizerFactory>();

            var assemblyName = new AssemblyName(ResourceType.GetTypeInfo().Assembly.FullName);
            var sharedLocalizer = localizerFactory.Create("SharedResource", assemblyName.Name);

            var localizedPropName = sharedLocalizer[LocalizerKey != null ? LocalizerKey : $"Localized{context.MemberName}"];

            if (Convert.ToInt32(value) <= MinValue)
            {
                ErrorMessage = FullMessage ? localizedPropName : sharedLocalizer["MinDecimalValueException", localizedPropName];
                return new ValidationResult(FormatErrorMessage(""));
            }

            return ValidationResult.Success;
        }
    }
}
