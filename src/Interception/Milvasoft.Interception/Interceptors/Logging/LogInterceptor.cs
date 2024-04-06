﻿using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.Response;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvasoft.Interception.Interceptors.Cache;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Milvasoft.Interception.Interceptors.Logging;

public partial class LogInterceptor(IServiceProvider serviceProvider) : IMilvaInterceptor
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IMilvaLogger _logger = serviceProvider.GetService<IMilvaLogger>();
    private readonly ILogInterceptionOptions _logInterceptionOptions = serviceProvider.GetService<ILogInterceptionOptions>();

    public int InterceptionOrder { get; set; } = -1;

    public async Task OnInvoke(Call call)
    {
        if (_logger == null)
        {
            await call.NextAsync();
            return;
        }

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

            if (call.ReturnValue is IResponse response)
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

            var methodParameters = call.Arguments?.ToList();

            methodParameters?.RemoveAll(p => p is CancellationToken);

            var logObjectPropDic = _logInterceptionOptions.LogDefaultParameters ? new Dictionary<string, object>()
            {
               { "TransactionId", ActivityHelper.TraceId },
               { "ElapsedMs", stopwatch.ElapsedMilliseconds },
               { "UtcLogTime" , DateTime.UtcNow },
               { "IsSuccess" , isSuccessResponse },
               { "Exception" , exception?.ToJson() },
               { "CacheInfo" , new {
                   FetchedFromCache = responseDataFetchedFromCache,
                   CacheRemoveKey = cacheAttribute?.Key,
                 }.ToJson()
               },
            } : [];

            ConfigurePropertyDictionaryWithMethodPropeties(logObjectPropDic, call);

            ConfigurePropertyDictionaryWithUserDefinedExtraProperties(logObjectPropDic);

            var logObjectAsJson = ConvertPropertyDictionaryToJson(logObjectPropDic);

            if (_logInterceptionOptions.AsyncLogging)
            {
                await _logger.LogAsync(logObjectAsJson);
            }
            else
                _logger.Log(logObjectAsJson);

            //If metadata removing requested, add removed metadata to call.returnValue again
            if (metadatas != null)
            {
                var returnVal = call.ReturnValue as IHasMetadata;
                returnVal.Metadatas = metadatas;
            }
        }

        if (exception != null)
        {
            throw exception;
        }
    }

    /// <summary>
    /// If user wants to log the values coming from the project where the library is used.
    /// </summary>
    /// <param name="logObjectPropDic"></param>
    private void ConfigurePropertyDictionaryWithUserDefinedExtraProperties(Dictionary<string, object> logObjectPropDic)
    {
        //If you want to log the values coming from the project where the library is used.
        var extraPropsObject = _logInterceptionOptions.ExtraLoggingPropertiesSelector?.Invoke(_serviceProvider);

        if (extraPropsObject != null)
        {
            var extraProps = extraPropsObject.GetType().GetProperties();

            foreach (var extraProp in extraProps)
                logObjectPropDic.Add(extraProp.Name, extraProp.GetValue(extraPropsObject, null));
        }
    }

    /// <summary>
    /// If <paramref name="call"/> hasn't <see cref="LogAttribute"/> this means call has <see cref="LogRunnerAttribute"/>. 
    /// So, get required values from InterceptorRunner.InterceptWithLogAsync.expression parameter.
    /// </summary>
    /// <param name="logObjectPropDic"></param>
    /// <param name="call"></param>
    private void ConfigurePropertyDictionaryWithMethodPropeties(Dictionary<string, object> logObjectPropDic, Call call)
    {
        if (!_logInterceptionOptions.LogDefaultParameters)
            return;

        var logAttribute = call.GetInterceptorAttribute<LogAttribute>();

        //If call hasn't LogAttribute this means call has LogRunnerAttribute. So, get required values from expression.
        if (logAttribute == null)
        {
            var expression = call.Arguments[0];

            var body = (MethodCallExpression)expression.GetType().GetProperty("Body").GetValue(expression);

            var argumentValues = new List<object>();

            foreach (var argument in body.Arguments)
                argumentValues.Add(argument.GetType().GetProperty("Value").GetValue(argument));

            argumentValues.RemoveAll(p => p is CancellationToken);

            var methodName = body.Method.Name;

            logObjectPropDic.Add("Namespace", body.Method.DeclaringType.Namespace);
            logObjectPropDic.Add("ClassName", body.Method.DeclaringType.Name);
            logObjectPropDic.Add("MethodName", methodName);
            logObjectPropDic.Add("MethodParams", argumentValues.ToJson());
        }
        else
        {
            var methodParameters = call.Arguments?.ToList();

            methodParameters?.RemoveAll(p => p is CancellationToken);

            logObjectPropDic.Add("Namespace", call.Method.DeclaringType.Namespace);
            logObjectPropDic.Add("ClassName", call.Method.DeclaringType.Name);
            logObjectPropDic.Add("MethodName", call.Method.Name);
            logObjectPropDic.Add("MethodParams", methodParameters?.ToJson());
        }

        logObjectPropDic.Add("MethodResult", call.ReturnValue?.ToJson());
    }

    /// <summary>
    /// Converts property dictionary with json. Removes extra trailing slashes.
    /// </summary>
    /// <param name="logObjectPropDic"></param>
    /// <returns></returns>
    private static string ConvertPropertyDictionaryToJson(Dictionary<string, object> logObjectPropDic)
    {
        var logObjectAsJson = logObjectPropDic.ToJson();

        logObjectAsJson = SlashRegex1().Replace(logObjectAsJson, " ");
        logObjectAsJson = SlashRegex2().Replace(logObjectAsJson, " ");

        logObjectAsJson = logObjectAsJson.Replace("\\\\\\\\", "\\\\");

        return logObjectAsJson;
    }

    [GeneratedRegex("([\\\\])([a-z])(\\d+)")]
    private static partial Regex SlashRegex1();

    [GeneratedRegex("(\\s(\\\\))|(((\\\\))\\s)")]
    private static partial Regex SlashRegex2();
}