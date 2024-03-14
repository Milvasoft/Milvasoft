using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core;
using Milvasoft.Core.Abstractions.Localization;
using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.Utils.Constants;
using Newtonsoft.Json;

namespace Milvasoft.Attributes.ActionFilter;

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
    /// Gets or sets required. Default value is true.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Gets or sets error message localization flag.
    /// </summary>
    public bool LocalizeErrorMessages { get; set; }

    /// <summary>
    /// Constructor of <see cref="ValidateIdParameterAttribute"/> for localization.
    /// </summary>
    public ValidateIdParameterAttribute() { }

    /// <summary>
    /// Performs when action executing.
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        async Task<ActionExecutingContext> RewriteResponseAsync(string errorMessage)
        {
            var validationResponse = new Response
            {
                IsSuccess = false,
                Messages = [new ResponseMessage(((int)MilvaException.Validation).ToString(), errorMessage, Components.Rest.Enums.MessageType.Error)],
                StatusCode = (int)MilvaStatusCodes.Status600Exception,
            };

            var json = JsonConvert.SerializeObject(validationResponse);
            context.HttpContext.Items.Add(new KeyValuePair<object, object>("StatusCode", MilvaStatusCodes.Status600Exception));
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = MilvaStatusCodes.Status200OK;
            await context.HttpContext.Response.WriteAsync(json).ConfigureAwait(false);

            context.Result = new OkResult();

            return context;
        };

        if (context.ActionArguments.Count != 0)
        {
            IMilvaLocalizer milvaLocalizer = null;

            if (LocalizeErrorMessages)
                milvaLocalizer = context.HttpContext.RequestServices.GetMilvaLocalizer();

            var message = milvaLocalizer != null
                     ? EntityName != null
                           ? milvaLocalizer[LocalizerKeys.ValidationIdPropertyError, milvaLocalizer[LocalizerKeys.LocalizedEntityName + EntityName]]
                           : milvaLocalizer[LocalizerKeys.ValidationIdParameterGeneralError]
                     : EntityName != null
                           ? $"{LocalizerKeys.PleaseEnterAValid} {EntityName}."
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
                    else if (valueType.GetGenericArguments()?.FirstOrDefault() == typeof(int))
                    {
                        var intParameters = (List<int>)parameterValue;

                        if (IsRequired && intParameters.IsNullOrEmpty())
                            base.OnActionExecuting(RewriteResponseAsync(message).Result);

                        foreach (var intParameter in intParameters)
                        {
                            var regexMatchResult = intParameter.ToString().MatchRegex("^[{]?[0-9a-fA-F]{8}"
                                                                                      + "-([0-9a-fA-F]{4}-)"
                                                                                      + "{3}[0-9a-fA-F]{12}[}]?$");

                            if (intParameter <= 0)
                                base.OnActionExecuting(RewriteResponseAsync(message).Result);
                        }
                    }
                    else if (valueType.GetGenericArguments()?.FirstOrDefault() == typeof(Guid))
                    {
                        var guidParameters = (List<Guid>)parameterValue;

                        if (IsRequired && guidParameters.IsNullOrEmpty())
                            base.OnActionExecuting(RewriteResponseAsync(message).Result);

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
