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

        #region Fields

        private readonly Type _resourceType = null;

        #endregion

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

        #endregion


        #region Constructors

        /// <summary>
        /// Constructor of atrribute.
        /// </summary>
        public MinDecimalValueAttribute() { }

        /// <summary>
        /// Constructor of atrribute.
        /// </summary>
        /// <param name="resourceType"></param>
        public MinDecimalValueAttribute(Type resourceType)
        {
            _resourceType = resourceType;
        }

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
        /// <param name="resourceType"></param>
        public MinDecimalValueAttribute(int minValue, Type resourceType)
        {
            MinValue = minValue;
            _resourceType = resourceType;
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
            IStringLocalizer sharedLocalizer;
            string localizedPropName;
            string errorMessage;

            if (_resourceType != null)
            {
                var localizerFactory = context.GetService<IStringLocalizerFactory>();

                var assemblyName = new AssemblyName(_resourceType.GetTypeInfo().Assembly.FullName);
                sharedLocalizer = localizerFactory.Create("SharedResource", assemblyName.Name);

                localizedPropName = sharedLocalizer[LocalizerKey != null ? LocalizerKey : $"Localized{context.MemberName}"];
                errorMessage = FullMessage ? sharedLocalizer[LocalizerKey] : sharedLocalizer["MinDecimalValueException", localizedPropName];
            }
            else errorMessage = $"Please enter a valid {context.MemberName}.";

            if (Convert.ToInt32(value) <= MinValue)
            {
                ErrorMessage = errorMessage;
                return new ValidationResult(FormatErrorMessage(""));
            }

            return ValidationResult.Success;
        }
    }
}
