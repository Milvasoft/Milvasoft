﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Milvasoft.DependencyInjection;
using Milvasoft.Helpers.Enums;
using Milvasoft.Helpers.Extensions;
using Milvasoft.Helpers.Models;
using Milvasoft.Helpers.Models.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Attributes.ActionFilter
{
    /// <summary>
    /// Specifies that the class or method that this attribute is applied to requires the specified prevent string injection attacks and min/max length checks.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PreventStringInjectionForParametersAttribute : ActionFilterAttribute
    {
        #region Properties

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
        /// Black list for string injections.
        /// </summary>
        public List<InvalidString> BlackList { get; set; }

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
        public PreventStringInjectionForParametersAttribute(int minimumLength, int maximumLength)
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
            var milvasoftLogger = context.HttpContext.RequestServices.GetRequiredService<IMilvasoftLogger>();

            var localizerFactory = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizerFactory>();

            var assemblyName = new AssemblyName(ResourceType.GetTypeInfo().Assembly.FullName);
            var sharedLocalizer = localizerFactory.Create("SharedResource", assemblyName.Name);

            async Task<ActionExecutingContext> RewriteResponseAsync(string errorMessage)
            {
                var localizedErrorMessage = errorMessage;

                var validationResponse = new ExceptionResponse
                {
                    Success = false,
                    Message = localizedErrorMessage,
                    StatusCode = MilvasoftStatusCodes.Status400BadRequest,
                    Result = new object(),
                    ErrorCodes = new List<int>()
                };
                var json = JsonConvert.SerializeObject(validationResponse);
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Items.Add(new KeyValuePair<object, object>("StatusCode", MilvasoftStatusCodes.Status600Exception));
                context.HttpContext.Response.StatusCode = MilvasoftStatusCodes.Status200OK;
                await context.HttpContext.Response.WriteAsync(json).ConfigureAwait(false);

                context.Result = new BadRequestResult();

                return context;
            };

            CommonHelper.EnsureLegalLengths(MaximumLength, MinimumLength, sharedLocalizer);

            if (context.ActionArguments.Count != 0)
            {
                foreach (var actionArgument in context.ActionArguments)
                {
                    if (actionArgument.Value is string)
                    {
                        var stringValue = actionArgument.Value.ToString();
                        var localizedPropName = actionArgument.Key;

                        // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
                        // We expect a cast exception if a non-string was passed in.
                        int length = actionArgument.Value == null ? 0 : stringValue.Length;
                        var lengthResult = actionArgument.Value == null || (length >= this.MinimumLength && length <= this.MaximumLength);

                        if (!lengthResult)
                        {
                            base.OnActionExecuting(RewriteResponseAsync(sharedLocalizer["PreventStringInjectionLengthResultNotTrue", localizedPropName, MinimumLength, MaximumLength]).Result);
                        }
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            if (!BlackList.IsNullOrEmpty())
                                foreach (var invalidString in BlackList)
                                    foreach (var invalidStringValue in invalidString.Values)
                                        if (stringValue.Contains(invalidStringValue))
                                        {
                                            if (!string.IsNullOrEmpty(MailContent))
                                                milvasoftLogger.LogFatal(MailContent, MailSubject.Hack);

                                            base.OnActionExecuting(RewriteResponseAsync(sharedLocalizer["PreventStringInjectionContainsForbiddenWordError", localizedPropName]).Result);
                                        }
                        }
                        if (MinimumLength > 0 && (stringValue?.Length ?? 0) < MinimumLength)
                        {
                            base.OnActionExecuting(RewriteResponseAsync(sharedLocalizer["PreventStringInjectionBellowMin", localizedPropName, MinimumLength]).Result);
                        }
                    }
                }
            }
        }

    }
}