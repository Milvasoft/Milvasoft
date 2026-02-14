using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Decorator.Internal;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvasoft.Interception.Interceptors.Cache;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Milvasoft.Interception.Interceptors.Logging;

/// <summary>
/// The LogInterceptor is an interceptor that allows the logging of method attributes, such as return values, parameters, and other properties, using the IMilvaLogger.
/// It intercepts methods that are marked with the LogAttribute and logs the relevant information using the provided logger.
/// </summary>
/// <param name="serviceProvider"></param>
public partial class LogInterceptor(IServiceProvider serviceProvider) : IMilvaInterceptor
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IMilvaLogger _logger = serviceProvider.GetService<IMilvaLogger>();
    private readonly ILogInterceptionOptions _logInterceptionOptions = serviceProvider.GetService<ILogInterceptionOptions>();

    /// <inheritdoc/>
    public int InterceptionOrder { get; set; } = -1;

    /// <inheritdoc/>
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
                {
                    var prop = call.ReturnValue?.GetType().GetProperty("IsCachedData");

                    if (prop != null && prop.PropertyType == typeof(bool))
                        responseDataFetchedFromCache = (bool)(prop.GetValue(call.ReturnValue) ?? false);
                }

                isSuccessResponse = response.IsSuccess;
            }

            List<ResponseDataMetadata> metadatas = null;

            if (call.ReturnValue is IHasMetadata res && _logInterceptionOptions.ExcludeResponseMetadataFromLog)
            {
                metadatas = res.Metadatas;
                res.Metadatas = null;
            }

            var logObjectPropDic = _logInterceptionOptions.LogDefaultParameters ? new Dictionary<string, object>()
            {
               { "TransactionId", ActivityHelper.TraceId },
               { "ElapsedMs", stopwatch.ElapsedMilliseconds },
               { "UtcLogTime" , CommonHelper.GetNow(_logInterceptionOptions.UseUtcForLogTimes) },
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

        (exception is AggregateException agg && agg.InnerExceptions.Count == 1 ? agg.InnerExceptions[0] : exception)?.Rethrow();
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
    /// Configures the property dictionary with method specific properties.
    /// </summary>
    /// <param name="logObjectPropDic">The property dictionary to be configured.</param>
    /// <param name="call">The call object containing the method information.</param>
    private void ConfigurePropertyDictionaryWithMethodPropeties(Dictionary<string, object> logObjectPropDic, Call call)
    {
        // Check if default parameters logging is enabled
        if (!_logInterceptionOptions.LogDefaultParameters)
            return;

        var logAttribute = call.GetInterceptorAttribute<LogAttribute>();

        // If the call does not have a LogAttribute, it means it has a LogRunnerAttribute. So, get the required values from the expression.
        if (logAttribute == null)
        {
            var expression = call.Arguments[0];

            var body = (MethodCallExpression)expression.GetType().GetProperty("Body").GetValue(expression);

            var argumentValues = new List<object>(body.Arguments.Count);

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
            var methodParameters = call.Arguments?.Where(p => p is not CancellationToken);

            logObjectPropDic.Add("Namespace", call.MethodImplementation.DeclaringType.Namespace);
            logObjectPropDic.Add("ClassName", call.MethodImplementation.DeclaringType.Name);
            logObjectPropDic.Add("MethodName", call.MethodImplementation.Name);
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

        return logObjectAsJson;
    }
}