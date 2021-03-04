using Milvasoft.Helpers.Attributes.Validation;
using System;

namespace Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes
{
    /// <summary>
    /// Specifies that the class or property that this attribute is applied to requires the specified the valid id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class OValidateIdAttribute : ValidateIdPropertyAttribute
    {
        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        public OValidateIdAttribute() : base(typeof(SharedResource)) { }
    }
}
