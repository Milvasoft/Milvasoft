using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models.Response;
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
        /// <summary>
        /// Gets or sets error message content.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Dummy class type for resource location.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Performs when action executing.
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var localizerFactory = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizerFactory>();

            var assemblyName = new AssemblyName(ResourceType.GetTypeInfo().Assembly.FullName);
            var sharedLocalizer = localizerFactory.Create("SharedResource", assemblyName.Name);

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

            var message = EntityName != null
                          ? sharedLocalizer["ValidationIdPropertyError", sharedLocalizer[$"LocalizedEntityName{EntityName}"]]
                          : sharedLocalizer["ValidationIdParameterGeneralError"];

            if (context.ActionArguments.Count != 0)
            {
                foreach (var actionArgument in context.ActionArguments)
                {
                    var modelName = string.IsNullOrEmpty(EntityName) ? actionArgument.Key : EntityName;
                    var propName = sharedLocalizer[modelName];

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
