﻿using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Core;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvasoft.Interception.Interceptors.Cache;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Milvasoft.Interception.Interceptors.Logging;

public partial class LogInterceptor : IMilvaInterceptor
{
    public static int InterceptionOrder { get; set; } = -1;

    private IServiceProvider _serviceProvider;
    private IMilvaLogger _logger;
    private ILogInterceptionOptions _logInterceptionOptions;

    public LogInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<IMilvaLogger>();
        _logInterceptionOptions = serviceProvider.GetService<ILogInterceptionOptions>();
    }

    public async Task OnInvoke(Call call)
    {
        var stopwatch = new Stopwatch();
        Exception exception = null;

        try
        {
            stopwatch.Start();

            await call.NextAsync();

        }
        catch (Exception ex)
        {
            exception = ex;
        }
        finally
        {
            stopwatch.Stop();

            bool isSuccessResponse = exception == null;
            bool responseDataFetchedFromCache = false;
            var cacheAttribute = call.GetInterceptorAttribute<CacheAttribute>();

            if (call.ReturnValue != null && call.ReturnValue is IResponse response)
            {
                if (cacheAttribute != null)
                    responseDataFetchedFromCache = (bool)call.ReturnValue.GetType().GetProperty("IsCachedData").GetValue(call.ReturnValue);

                isSuccessResponse = response.IsSuccess;
            }

            List<ResponseDataMetadata> metadatas = null;

            if (call.ReturnValue is IHasMetadata res && _logInterceptionOptions.ExcludeResponseMetadataFromLog)
            {
                metadatas = res.Metadatas;
                res.Metadatas = null;
            }

            string exceptionAsJson = null;

            if (exception != null)
            {
                var converter = new ExceptionConverter<Exception>();

                var jsonOpts = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull | System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
                };
                jsonOpts.Converters.Add(converter);

                exceptionAsJson = exception.ToJson(jsonOpts);
            }

            var logObjectPropDic = _logInterceptionOptions.LogDefaultParameters ? new Dictionary<string, object>()
            {
               { "TransactionId", ActivityHelper.TraceId },
               { "MethodName", call.Method.Name },
               { "MethodParams", call.Arguments?.ToJson() },
               { "MethodResult", call.ReturnValue?.ToJson() },
               { "ElapsedMs", stopwatch.ElapsedMilliseconds },
               { "UtcLogTime" , DateTime.UtcNow },
               { "IsSuccess" , isSuccessResponse },
               { "Exception" , exceptionAsJson },
               { "CacheInfo" , new {
                   FetchedFromCache = responseDataFetchedFromCache,
                   CacheRemoveKey = cacheAttribute?.Key,
                 }.ToJson()
               },
            } : [];

            var logAttribute = call.GetInterceptorAttribute<LogAttribute>();

            //If call hasn't LogAttribute this means call has LogRunnerAttribute. So, get required values from expression.
            if (logAttribute == null && _logInterceptionOptions.LogDefaultParameters)
            {
                var expression = call.Arguments[0];

                var body = (MethodCallExpression)expression.GetType().GetProperty("Body").GetValue(expression);

                var values = new List<object>();

                foreach (var argument in body.Arguments)
                    values.Add(argument.GetType().GetProperty("Value").GetValue(argument));

                var argumentValues = values;

                var methodName = body.Method.Name;

                logObjectPropDic["MethodName"] = methodName;
                logObjectPropDic["MethodParams"] = argumentValues;
            }

            //If you want to log the values coming from the project where the library is used.
            var extraPropsObject = _logInterceptionOptions.ExtraLoggingPropertiesSelector?.Invoke(_serviceProvider);

            if (extraPropsObject != null)
            {
                var extraProps = extraPropsObject.GetType().GetProperties();

                foreach (var extraProp in extraProps)
                {
                    logObjectPropDic.Add(extraProp.Name, extraProp.GetValue(extraPropsObject, null));
                }
            }

            if (_logInterceptionOptions.AsyncLogging)
            {
                var logObjectAsJson = logObjectPropDic.ToJson();

                logObjectAsJson = SlashRegex1().Replace(logObjectAsJson, " ");
                logObjectAsJson = SlashRegex2().Replace(logObjectAsJson, " ");

                logObjectAsJson = logObjectAsJson.Replace("\\\\\\\\", "\\\\");

                await _logger.LogAsync(logObjectAsJson);
            }
            else _logger.Log(logObjectPropDic.ToJson());

            //If metadata removing requested, add removed metadata to call.returnValue again
            if (metadatas != null)
            {
                var returnVal = call.ReturnValue as IHasMetadata;
                returnVal.Metadatas = metadatas;
            }

            if (exception != null)
            {
                throw exception;
            }
        }
    }


    [GeneratedRegex("([\\\\])([a-z])(\\d+)")]
    private static partial Regex SlashRegex1();

    [GeneratedRegex("(\\s(\\\\))|(((\\\\))\\s)")]
    private static partial Regex SlashRegex2();
}