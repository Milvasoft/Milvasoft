using Milvasoft.Helpers.Attributes.Validation;
using Milvasoft.SampleAPI.Localization;
using System;

namespace Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes
{
    /// <summary>
    /// Specifies that the class or property that this attribute is applied to requires the specified prevent string injection attacks and min/max length checks.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class OValidateStringAttribute : ValidateStringAttribute
    {
        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        public OValidateStringAttribute(int maximumLength) : base(maximumLength)
        {
            base.MailContent = GlobalConstants.MailContent;
            base.ResourceType = typeof(SharedResource);
        }

        /// <summary>
        /// Constructor that accepts the maximum length of the string.
        /// </summary>
        /// <param name="minimumLength">The minimum length, inclusive.  It may not be negative.</param>
        /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
        public OValidateStringAttribute(int minimumLength, int maximumLength) : base(minimumLength, maximumLength)
        {
            base.MailContent = GlobalConstants.MailContent;
            base.ResourceType = typeof(SharedResource);
        }
    }
}
