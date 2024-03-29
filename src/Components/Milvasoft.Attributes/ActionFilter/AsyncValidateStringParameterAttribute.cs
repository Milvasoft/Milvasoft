﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Utils.Models;

namespace Milvasoft.Attributes.ActionFilter;

/// <summary>
/// Throws <see cref="MilvaUserFriendlyException"/> if string parameter is not valid. Specifies that the class or method that this attribute is applied to requires the specified prevent string injection attacks and min/max length checks.
/// </summary>
/// <remarks>
/// Constructor that accepts the maximum length of the string.
/// </remarks>
/// <param name="minimumLength">The minimum length, inclusive.  It may not be negative.</param>
/// <param name="maximumLength">The maximum length, inclusive.  It may not be negative.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AsyncValidateStringParameterAttribute(int minimumLength, int maximumLength) : Attribute, IAsyncActionFilter
{
    #region Properties

    /// <summary>
    /// Gets or sets the error message spesific content
    /// </summary>
    public string MemberNameLocalizerKey { get; set; }

    /// <summary>
    /// Gets the maximum acceptable length of the string
    /// </summary>
    public int MaximumLength { get; private set; } = maximumLength;

    /// <summary>
    /// Gets or sets the minimum acceptable length of the string
    /// </summary>
    public int MinimumLength { get; private set; } = minimumLength;

    /// <summary>
    /// Gets or sets error message localization flag.
    /// </summary>
    public bool LocalizeErrorMessages { get; set; }

    /// <summary>
    /// If injection attack exist. Validation method will be send mail. 
    /// If has value mail will be send.
    /// </summary>
    public string MailContent { get; set; }

    #endregion

    /// <summary>
    /// Performs when action executing.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    public virtual async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        IMilvaLocalizer milvaLocalizer = null;

        if (LocalizeErrorMessages)
            milvaLocalizer = context.HttpContext.RequestServices.GetService<IMilvaLocalizer>();

        //CommonHelper.EnsureLegalLengths(MaximumLength, MinimumLength, milvaLocalizer);

        if (context.ActionArguments.Count != 0)
        {
            foreach (var actionArgument in context.ActionArguments)
            {
                if (actionArgument.Value is string)
                {
                    var stringValue = actionArgument.Value.ToString();
                    var localizedPropName = milvaLocalizer[MemberNameLocalizerKey ?? $"{LocalizerKeys.Localized}{actionArgument.Key}"];

                    // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
                    // We expect a cast exception if a non-string was passed in.
                    int length = actionArgument.Value == null ? 0 : stringValue.Length;
                    var lengthResult = actionArgument.Value == null || length >= MinimumLength && length <= MaximumLength;

                    if (!lengthResult)
                    {
                        throw new MilvaUserFriendlyException(milvaLocalizer != null
                                                                    ? milvaLocalizer[LocalizerKeys.PreventStringInjectionLengthResultNotTrue, localizedPropName, MinimumLength, MaximumLength]
                                                                    : $"{localizedPropName} must have a character length in the range {MinimumLength} to {MaximumLength}.", MilvaException.Validation);
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
                                            milvasoftLogger.Error(MailContent);

                                        throw new MilvaUserFriendlyException(milvaLocalizer != null
                                                                                    ? milvaLocalizer[LocalizerKeys.PreventStringInjectionContainsForbiddenWordError, localizedPropName]
                                                                                    : $"{localizedPropName} contains invalid words.", MilvaException.Validation);
                                    }
                    }

                    if (MinimumLength > 0 && (stringValue?.Length ?? 0) < MinimumLength)
                    {
                        throw new MilvaUserFriendlyException(milvaLocalizer != null
                                                                    ? milvaLocalizer[LocalizerKeys.PreventStringInjectionBellowMin, localizedPropName, MinimumLength]
                                                                    : $"{localizedPropName} is below the minimum character limit. Please enter at least {MinimumLength} characters.", MilvaException.Validation);
                    }
                }
            }
        }

        await next();
    }
}
