using Microsoft.AspNetCore.Mvc.Filters;

namespace Milvasoft.Attributes.ActionFilter;

/// <summary>
/// Throws <see cref="MilvaUserFriendlyException"/> if id parameter is not valid. Specifies that the class or method that this attribute is applied to requires the specified the valid id.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AsyncValidateIdParameterAttribute : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// Gets or sets error message content.
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Gets or sets error message localization flag.
    /// </summary>
    public bool LocalizeErrorMessages { get; set; }

    /// <summary>
    /// Constructor of <see cref="ValidateIdParameterAttribute"/> for localization.
    /// </summary>
    public AsyncValidateIdParameterAttribute() { }

    /// <summary>
    /// Performs when action executing.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    public virtual async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
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
                        {
                            throw new MilvaUserFriendlyException(message, MilvaException.Validation);
                        }
                    }
                    else if (valueType == typeof(int))
                    {
                        var intParameter = (int)parameterValue;

                        if (intParameter <= 0)
                        {
                            throw new MilvaUserFriendlyException(message, MilvaException.Validation);
                        }

                    }
                    else if (valueType.GetGenericArguments()?.FirstOrDefault() == typeof(int))
                    {
                        var intParameters = (List<int>)parameterValue;

                        if (intParameters.IsNullOrEmpty())
                        {
                            throw new MilvaUserFriendlyException(message, MilvaException.Validation);
                        }

                        foreach (var intParameter in intParameters)
                        {
                            var regexMatchResult = intParameter.ToString().MatchRegex("^[{]?[0-9a-fA-F]{8}"
                                                                                      + "-([0-9a-fA-F]{4}-)"
                                                                                      + "{3}[0-9a-fA-F]{12}[}]?$");

                            if (intParameter <= 0)
                            {
                                throw new MilvaUserFriendlyException(message, MilvaException.Validation);
                            }
                        }
                    }
                    else if (valueType.GetGenericArguments()?.FirstOrDefault() == typeof(Guid))
                    {
                        var guidParameters = (List<Guid>)parameterValue;

                        if (guidParameters.IsNullOrEmpty())
                        {
                            throw new MilvaUserFriendlyException(message, MilvaException.Validation);
                        }

                        foreach (var guidParameter in guidParameters)
                        {
                            var regexMatchResult = guidParameter.ToString().MatchRegex("^[{]?[0-9a-fA-F]{8}"
                                                                                      + "-([0-9a-fA-F]{4}-)"
                                                                                      + "{3}[0-9a-fA-F]{12}[}]?$");

                            if (guidParameter == default || guidParameter == Guid.Empty || !regexMatchResult)
                            {
                                throw new MilvaUserFriendlyException(message, MilvaException.Validation);
                            }
                        }
                    }
                }
            }
        }

        await next();
    }
}
