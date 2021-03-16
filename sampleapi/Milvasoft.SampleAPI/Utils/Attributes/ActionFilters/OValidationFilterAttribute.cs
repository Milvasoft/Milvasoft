using Microsoft.AspNetCore.Mvc.Filters;
using Milvasoft.Helpers.Attributes.ActionFilter;
using Milvasoft.SampleAPI.DTOs.AccountDTOs;
using System;

namespace Milvasoft.SampleAPI.Utils.Attributes.ActionFilters
{
    /// <summary>
    ///  Provides the attribute validation exclude opportunity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OValidationFilterAttribute : ValidationFilterAttribute
    {
        /// <summary>
        /// Performs when action executing.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.DTOFolderAssemblyName = "Milvasoft.SampleAPI.DTOs";
            base.AssemblyTypeForNestedProps = typeof(LoginDTO);
            base.OnActionExecuting(context);
        }
    }
}
