using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Attributes.ActionFilter
{
    /// <summary>
    /// Specifies that the class or method that this attribute is applied to requires the specified the valid id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateIdParameterAttribute : ActionFilterAttribute
    {

        #region Fields

        private readonly Type _resourceType = null;

        #endregion

        /// <summary>
        /// Gets or sets error message content.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Constructor of <see cref="ValidateIdParameterAttribute"/> for localization.
        /// </summary>
        public ValidateIdParameterAttribute() { }

        /// <summary>
        /// Constructor of <see cref="ValidateIdParameterAttribute"/> for localization.
        /// </summary>
        /// <param name="resourceType"></param>
        public ValidateIdParameterAttribute(Type resourceType)
        {
            _resourceType = resourceType;
        }

        /// <summary>
        /// Performs when action executing.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            async Task<ActionExecutingContext> RewriteResponseAsync(string errorMessage)
            {
                var validationResponse = new ExceptionResponse
                {
                    Success = false,
                    Message = errorMessage,
                    StatusCode = MilvasoftStatusCodes.Status600Exception,
                    Result = new object(),
                    ErrorCodes = new List<int>()
                };
                var json = JsonConvert.SerializeObject(validationResponse);
                context.HttpContext.Items.Add(new KeyValuePair<object, object>("StatusCode", MilvasoftStatusCodes.Status600Exception));
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = MilvasoftStatusCodes.Status200OK;
                await context.HttpContext.Response.WriteAsync(json).ConfigureAwait(false);

                context.Result = new OkResult();

                return context;
            };


            if (context.ActionArguments.Count != 0)
            {
                IStringLocalizer sharedLocalizer = null;
                if (_resourceType != null)
                {
                    var localizerFactory = context.HttpContext.RequestServices.GetService<IStringLocalizerFactory>();

                    var assemblyName = new AssemblyName(_resourceType.GetTypeInfo().Assembly.FullName);
                    sharedLocalizer = localizerFactory.Create(_resourceType.Name, assemblyName.Name);
                }

                var message = sharedLocalizer != null
                         ? EntityName != null
                               ? sharedLocalizer[LocalizerKeys.ValidationIdPropertyError, sharedLocalizer[LocalizerKeys.LocalizedEntityName + EntityName]]
                               : sharedLocalizer[LocalizerKeys.ValidationIdParameterGeneralError]
                         : EntityName != null
                               ? $"Please select a valid {EntityName}."
                               : "Please enter all required parameters completely.";


                foreach (var actionArgument in context.ActionArguments)
                {
                    if (actionArgument.Key.Contains("Id") || actionArgument.Key.Contains("id"))
                    {
                        var parameterValue = actionArgument.Value;

                        var valueType = parameterValue.GetType();

                        if (valueType == typeof(Guid))
                        {
                            var guidParameter = (Guid)parameterValue;

                            var regexMatchResult = guidParameter.ToString().MatchRegex("^[{]?[0-9a-fA-F]{8}"
                                                                                      + "-([0-9a-fA-F]{4}-)"
                                                                                      + "{3}[0-9a-fA-F]{12}[}]?$");

                            if (guidParameter == default || guidParameter == Guid.Empty || !regexMatchResult)
                                base.OnActionExecuting(RewriteResponseAsync(message).Result);
                        }
                        else if (valueType == typeof(int))
                        {
                            var intParameter = (int)parameterValue;

                            if (intParameter <= 0)
                                base.OnActionExecuting(RewriteResponseAsync(message).Result);
                        }
                        else if (valueType.GetGenericArguments()[0] == typeof(int))
                        {
                            var intParameters = (List<int>)parameterValue;

                            if (intParameters.IsNullOrEmpty()) base.OnActionExecuting(RewriteResponseAsync(message).Result);

                            foreach (var intParameter in intParameters)
                            {
                                var regexMatchResult = intParameter.ToString().MatchRegex("^[{]?[0-9a-fA-F]{8}"
                                                                                          + "-([0-9a-fA-F]{4}-)"
                                                                                          + "{3}[0-9a-fA-F]{12}[}]?$");

                                if (intParameter <= 0)
                                    base.OnActionExecuting(RewriteResponseAsync(message).Result);
                            }
                        }
                        else if (valueType.GetGenericArguments()[0] == typeof(Guid))
                        {
                            var guidParameters = (List<Guid>)parameterValue;

                            if (guidParameters.IsNullOrEmpty()) base.OnActionExecuting(RewriteResponseAsync(message).Result);

                            foreach (var guidParameter in guidParameters)
                            {
                                var regexMatchResult = guidParameter.ToString().MatchRegex("^[{]?[0-9a-fA-F]{8}"
                                                                                          + "-([0-9a-fA-F]{4}-)"
                                                                                          + "{3}[0-9a-fA-F]{12}[}]?$");

                                if (guidParameter == default || guidParameter == Guid.Empty || !regexMatchResult)
                                    base.OnActionExecuting(RewriteResponseAsync(message).Result);
                            }

                        }
                    }
                }
            }
        }
    }
}
