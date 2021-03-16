using Microsoft.AspNetCore.Mvc.Filters;
using Milvasoft.Helpers.Attributes.ActionFilter;
using Milvasoft.SampleAPI.Localization;
using System;

namespace Milvasoft.SampleAPI.Utils.Attributes.ActionFilters
{
    /// <summary>
    /// Specifies that the class or method that this attribute is applied to requires the specified prevent string injection attacks and min/max length checks.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OValidateStringParameterAttribute : ValidateStringParameterAttribute
    {
        /// <summary>
        /// Constructor of <see cref="OValidateStringParameterAttribute"/> for localization.
        /// </summary>
        public OValidateStringParameterAttribute(int minimumLength, int maximumLength) : base(minimumLength, maximumLength)
        {
            base.ResourceType = typeof(SharedResource);
            base.MailContent = GlobalConstants.MailContent;
        }

        /// <summary>
        /// Performs when action executing.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }
}
