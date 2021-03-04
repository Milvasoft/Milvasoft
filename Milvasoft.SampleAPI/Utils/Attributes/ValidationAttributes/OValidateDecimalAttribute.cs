using Milvasoft.Helpers.Attributes.Validation;
using System;

namespace Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes
{
    /// <summary>
    /// Determines minimum decimal value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class OValidateDecimalAttribute : ValidateDecimalAttribute
    {
        /// <summary>
        /// Constructor of atrribute.
        /// </summary>
        public OValidateDecimalAttribute() : base(typeof(SharedResource)) { }

        /// <summary>
        /// Constructor of atrribute.
        /// </summary>
        /// <param name="minValue"></param>
        public OValidateDecimalAttribute(int minValue) : base(minValue, typeof(SharedResource)) { }
    }
}
