using Microsoft.AspNetCore.Mvc.Filters;
using Milvasoft.Helpers.Attributes.ActionFilter;
using Milvasoft.SampleAPI.Localization;
using System;

namespace Milvasoft.SampleAPI.Utils.Attributes.ActionFilters
{
    /// <summary>
    /// Specifies that the class or method that this attribute is applied to requires the specified the valid id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OValidateIdParameterAttribute : ValidateIdParameterAttribute
    {
        /// <summary>
        /// Constructor of <see cref="OValidateIdParameterAttribute"/> for localization.
        /// </summary>
        public OValidateIdParameterAttribute() : base(typeof(SharedResource)) { }

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
