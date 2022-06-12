using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.Core;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Extensions;
using Milvasoft.Core.Utils.Constants;
using Milvasoft.Core.Utils.Enums;
using Milvasoft.Core.Utils.Models;
using Milvasoft.Core.Utils.Models.Response;
using Newtonsoft.Json;

namespace Milvasoft.Attributes.ActionFilter;

/// <summary>
/// Specifies that the class or method that this attribute is applied to requires the specified prevent string injection attacks and min/max length checks.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidateStringParameterAttribute : ActionFilterAttribute
{

    #region Properties

    /// <summary>
    /// Gets or sets the error message spesific content
    /// </summary>
    public string MemberNameLocalizerKey { get; set; }

    /// <summary>
    /// Gets the maximum acceptable length of the string
    /// </summary>
    public int MaximumLength { get; private set; }

    /// <summary>
    /// Gets or sets the minimum acceptable length of the string
    /// </summary>
    public int MinimumLength { get; private set; } = 0;

    /// <summary>
    /// Dummy class type for resource location.
    /// </summary>
    public Type ResourceType { get; set; }

    /// <summary>
    /// If injection attack exist. Validation method will be send mail. 
    /// If has value mail will be send.
    /// </summary>
    public string MailContent { get; set; }

    #endregion


    /// <summary>
    /// Constructor that accepts the maximum length of the string.
    /// </summary>
    /// <param name="minimumLength">The minimum length, inclusive.  It may not be negative.</param>
    /// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
    public ValidateStringParameterAttribute(int minimumLength, int maximumLength)
    {
        MaximumLength = maximumLength;
        MinimumLength = minimumLength;
    }

    /// <summary>
    /// Performs when action executing.
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        IStringLocalizer sharedLocalizer = null;

        if (ResourceType != null)
            sharedLocalizer = context.HttpContext.RequestServices.GetLocalizerInstance(ResourceType);

        CommonHelper.EnsureLegalLengths(MaximumLength, MinimumLength, sharedLocalizer);

        if (context.ActionArguments.Count != 0)
        {
            foreach (var actionArgument in context.ActionArguments)
            {
                if (actionArgument.Value is string)
                {
                    var stringValue = actionArgument.Value.ToString();
                    var localizedPropName = MemberNameLocalizerKey ?? actionArgument.Key;

                    // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
                    // We expect a cast exception if a non-string was passed in.
                    int length = actionArgument.Value == null ? 0 : stringValue.Length;
                    var lengthResult = actionArgument.Value == null || length >= MinimumLength && length <= MaximumLength;

                    if (!lengthResult)
                    {
                        base.OnActionExecuting(RewriteResponseAsync(sharedLocalizer != null
                                                                    ? sharedLocalizer[LocalizerKeys.PreventStringInjectionLengthResultNotTrue, localizedPropName, MinimumLength, MaximumLength]
                                                                    : $"{localizedPropName} must have a character length in the range {MinimumLength} to {MaximumLength}.").Result);
                    }
                    if (!string.IsNullOrWhiteSpace(stringValue))
                    {
                        var blackList = context.HttpContext.RequestServices.GetService<List<InvalidString>>();

                        if (!blackList.IsNullOrEmpty())
                            foreach (var invalidString in blackList)
                                foreach (var invalidStringValue in invalidString.Values)
                                    if (stringValue.Contains(invalidStringValue))
                                    {
                                        var milvasoftLogger = context.HttpContext.RequestServices.GetService<IMilvaLogger>();

                                        if (!string.IsNullOrWhiteSpace(MailContent) && milvasoftLogger != null)
                                            milvasoftLogger.LogFatal(MailContent, MailSubject.Hack);

                                        base.OnActionExecuting(RewriteResponseAsync(sharedLocalizer != null
                                                                                    ? sharedLocalizer[LocalizerKeys.PreventStringInjectionContainsForbiddenWordError, localizedPropName]
                                                                                    : $"{localizedPropName} contains invalid words.").Result);
                                    }
                    }
                    if (MinimumLength > 0 && (stringValue?.Length ?? 0) < MinimumLength)
                    {
                        base.OnActionExecuting(RewriteResponseAsync(sharedLocalizer != null
                                                                    ? sharedLocalizer[LocalizerKeys.PreventStringInjectionBellowMin, localizedPropName, MinimumLength]
                                                                    : $"{localizedPropName} is below the minimum character limit. Please enter at least {MinimumLength} characters.").Result);
                    }
                }
            }
        }

        async Task<ActionExecutingContext> RewriteResponseAsync(string errorMessage)
        {
            var localizedErrorMessage = errorMessage;

            var validationResponse = new ExceptionResponse
            {
                Success = false,
                Message = localizedErrorMessage,
                StatusCode = MilvaStatusCodes.Status400BadRequest,
                Result = new object(),
                ErrorCodes = new List<int>()
            };

            var json = JsonConvert.SerializeObject(validationResponse);
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Items.Add(new KeyValuePair<object, object>("StatusCode", MilvaStatusCodes.Status600Exception));
            context.HttpContext.Response.StatusCode = MilvaStatusCodes.Status200OK;
            await context.HttpContext.Response.WriteAsync(json).ConfigureAwait(false);

            context.Result = new BadRequestResult();

            return context;
        };
    }

}
