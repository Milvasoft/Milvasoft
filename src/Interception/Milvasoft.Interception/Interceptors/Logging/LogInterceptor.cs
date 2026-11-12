using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Utils.Converters;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvasoft.Interception.Interceptors.Cache;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Milvasoft.Interception.Interceptors.Logging;

/// <summary>
/// The LogInterceptor is an interceptor that allows the logging of method attributes, such as return values, parameters, and other properties, using the IMilvaLogger.
/// It intercepts methods that are marked with the LogAttribute and logs the relevant information using the provided logger.
/// </summary>
/// <remarks>
/// Logging never changes the outcome of the intercepted call: failures while building or writing the log entry are swallowed
/// (a minimal fallback entry is attempted) and the call's result or exception is returned unchanged.
/// </remarks>
/// <param name="serviceProvider"></param>
public partial class LogInterceptor(IServiceProvider serviceProvider) : IMilvaInterceptor
{
    private const string _notSerializedPlaceholder = "<not serialized>";

    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IMilvaLogger _logger = serviceProvider.GetService<IMilvaLogger>();
    private readonly ILogInterceptionOptions _logInterceptionOptions = serviceProvider.GetService<ILogInterceptionOptions>() ?? new LogInterceptionOptions();

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

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await call.NextAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await SafeLogAsync(call, ex, stopwatch.ElapsedMilliseconds);

            throw;
        }

        stopwatch.Stop();

        await SafeLogAsync(call, null, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Builds and writes the log entry. Never throws.
    /// </summary>
    private async Task SafeLogAsync(Call call, Exception exception, long elapsedMilliseconds)
    {
        string logEntry;

        try
        {
            logEntry = BuildLogEntry(call, exception, elapsedMilliseconds);
        }
        catch (Exception buildException)
        {
            logEntry = BuildFallbackLogEntry(call, exception, buildException, elapsedMilliseconds);
        }

        if (logEntry == null)
            return;

        try
        {
            // Synchronous methods block until interceptors complete, so they are logged synchronously.
            if (_logInterceptionOptions.AsyncLogging && call.IsAsync)
                await _logger.LogAsync(logEntry);
            else
                _logger.Log(logEntry);
        }
        catch (Exception)
        {
            // Logging must never change the outcome of the call (or roll back committed work in outer interceptors).
        }
    }

    private string BuildLogEntry(Call call, Exception exception, long elapsedMilliseconds)
    {
        bool isSuccessResponse = exception == null;
        bool responseDataFetchedFromCache = false;
        var cacheAttribute = call.GetInterceptorAttribute<CacheAttribute>();

        if (call.ReturnValue is IResponse response)
        {
            if (cacheAttribute != null)
                responseDataFetchedFromCache = IsCachedData(call.ReturnValue);

            isSuccessResponse = exception == null && response.IsSuccess;
        }

        var maskingOptions = GetMaskingSerializerOptions();

        var logObjectPropDic = _logInterceptionOptions.LogDefaultParameters ? new Dictionary<string, object>()
        {
           { "TransactionId", ActivityHelper.TraceId },
           { "ElapsedMs", elapsedMilliseconds },
           { "UtcLogTime" , CommonHelper.GetNow(_logInterceptionOptions.UseUtcForLogTimes) },
           { "IsSuccess" , isSuccessResponse },
           { "Exception" , SerializeException(exception) },
           { "CacheInfo" , new {
               FetchedFromCache = responseDataFetchedFromCache,
               CacheRemoveKey = cacheAttribute?.Key,
             }.ToJson()
           },
        } : [];

        ConfigurePropertyDictionaryWithMethodPropeties(logObjectPropDic, call, maskingOptions);

        ConfigurePropertyDictionaryWithUserDefinedExtraProperties(logObjectPropDic);

        return logObjectPropDic.ToJson(maskingOptions);
    }

    /// <summary>
    /// Builds a minimal entry when the full entry cannot be built (for example when a value cannot be serialized).
    /// </summary>
    private string BuildFallbackLogEntry(Call call, Exception exception, Exception buildException, long elapsedMilliseconds)
    {
        try
        {
            return new Dictionary<string, object>
            {
                { "TransactionId", ActivityHelper.TraceId },
                { "ElapsedMs", elapsedMilliseconds },
                { "UtcLogTime" , CommonHelper.GetNow(_logInterceptionOptions.UseUtcForLogTimes) },
                { "IsSuccess" , exception == null && (call.ReturnValue is not IResponse response || response.IsSuccess) },
                { "Exception" , SerializeException(exception) },
                { "Namespace", call.MethodImplementation?.DeclaringType?.Namespace },
                { "ClassName", call.MethodImplementation?.DeclaringType?.Name },
                { "MethodName", call.MethodImplementation?.Name },
                { "LogError", $"Log entry could not be built: {buildException.GetType().Name}: {buildException.Message}" },
            }.ToJson();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsCachedData(object returnValue)
    {
        var prop = returnValue.GetType().GetProperty("IsCachedData");

        return prop != null && prop.PropertyType == typeof(bool) && (bool)(prop.GetValue(returnValue) ?? false);
    }

    /// <summary>
    /// If user wants to log the values coming from the project where the library is used.
    /// Extra properties with the same name as a default property replace it.
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
                logObjectPropDic[extraProp.Name] = extraProp.GetValue(extraPropsObject, null);
        }
    }

    /// <summary>
    /// Configures the property dictionary with method specific properties.
    /// </summary>
    /// <param name="logObjectPropDic">The property dictionary to be configured.</param>
    /// <param name="call">The call object containing the method information.</param>
    /// <param name="maskingOptions">Serializer options which apply <see cref="LogMaskedAttribute"/>.</param>
    private void ConfigurePropertyDictionaryWithMethodPropeties(Dictionary<string, object> logObjectPropDic, Call call, JsonSerializerOptions maskingOptions)
    {
        // Check if default parameters logging is enabled
        if (!_logInterceptionOptions.LogDefaultParameters)
            return;

        var logAttribute = call.GetInterceptorAttribute<LogAttribute>();

        // If the call does not have a LogAttribute, it means it has a LogRunnerAttribute. So, get the required values from the expression.
        if (logAttribute == null && call.Arguments?.Length > 0 && call.Arguments[0] is LambdaExpression lambda)
        {
            var body = UnwrapConvert(lambda.Body);

            if (body is MethodCallExpression methodCall)
            {
                var argumentValues = GetMethodCallArgumentValues(methodCall);

                logObjectPropDic["Namespace"] = methodCall.Method.DeclaringType?.Namespace;
                logObjectPropDic["ClassName"] = methodCall.Method.DeclaringType?.Name;
                logObjectPropDic["MethodName"] = methodCall.Method.Name;
                logObjectPropDic["MethodParams"] = SerializeValue(argumentValues, maskingOptions);
            }
            else
            {
                logObjectPropDic["Namespace"] = null;
                logObjectPropDic["ClassName"] = null;
                logObjectPropDic["MethodName"] = body.ToString();
                logObjectPropDic["MethodParams"] = null;
            }
        }
        else
        {
            var methodParameters = GetMaskedArguments(call.MethodImplementation, call.Arguments);

            logObjectPropDic["Namespace"] = call.MethodImplementation.DeclaringType?.Namespace;
            logObjectPropDic["ClassName"] = call.MethodImplementation.DeclaringType?.Name;
            logObjectPropDic["MethodName"] = call.MethodImplementation.Name;
            logObjectPropDic["MethodParams"] = SerializeValue(methodParameters, maskingOptions);
        }

        logObjectPropDic["MethodResult"] = call.ReturnValue == null ? null : SerializeValue(call.ReturnValue, maskingOptions, excludeMetadata: _logInterceptionOptions.ExcludeResponseMetadataFromLog);
    }

    /// <summary>
    /// Gets the arguments to log. Cancellation tokens are skipped and parameters marked with <see cref="LogMaskedAttribute"/> are replaced with the mask.
    /// </summary>
    private static List<object> GetMaskedArguments(MethodInfo method, object[] arguments)
    {
        if (arguments == null)
            return null;

        var parameters = method?.GetParameters() ?? [];
        var values = new List<object>(arguments.Length);

        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i] is CancellationToken)
                continue;

            var maskAttribute = i < parameters.Length ? parameters[i].GetCustomAttribute<LogMaskedAttribute>() : null;

            values.Add(maskAttribute != null && arguments[i] != null ? maskAttribute.Mask : ToLoggableValue(arguments[i]));
        }

        return values;
    }

    /// <summary>
    /// Reads the argument values of a runner expression without evaluating arbitrary code (no method calls are executed twice).
    /// Constants and captured variables are read; other argument expressions are logged as their expression text.
    /// </summary>
    private static List<object> GetMethodCallArgumentValues(MethodCallExpression methodCall)
    {
        var parameters = methodCall.Method.GetParameters();
        var values = new List<object>(methodCall.Arguments.Count);

        for (int i = 0; i < methodCall.Arguments.Count; i++)
        {
            var value = TryReadValue(methodCall.Arguments[i], out var readValue) ? readValue : methodCall.Arguments[i].ToString();

            if (value is CancellationToken)
                continue;

            var maskAttribute = i < parameters.Length ? parameters[i].GetCustomAttribute<LogMaskedAttribute>() : null;

            values.Add(maskAttribute != null && value != null ? maskAttribute.Mask : ToLoggableValue(value));
        }

        return values;
    }

    private static bool TryReadValue(Expression expression, out object value)
    {
        value = null;

        switch (UnwrapConvert(expression))
        {
            case ConstantExpression constant:
                value = constant.Value;
                return true;

            case MemberExpression member:
                object instance = null;

                if (member.Expression != null && !TryReadValue(member.Expression, out instance))
                    return false;

                if (member.Expression != null && instance == null)
                    return false;

                switch (member.Member)
                {
                    case FieldInfo field:
                        value = field.GetValue(instance);
                        return true;
                    case PropertyInfo property when property.GetIndexParameters().Length == 0:
                        value = property.GetValue(instance);
                        return true;
                    default:
                        return false;
                }

            default:
                return false;
        }
    }

    private static Expression UnwrapConvert(Expression expression)
    {
        while (expression is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary)
            expression = unary.Operand;

        return expression;
    }

    /// <summary>
    /// Serializes a value for the log. Values that would execute work when enumerated (queries, async streams, streams) are not serialized.
    /// </summary>
    private static string SerializeValue(object value, JsonSerializerOptions options, bool excludeMetadata = false)
    {
        if (value == null)
            return null;

        if (IsNotSerializable(value))
            return $"{_notSerializedPlaceholder} ({value.GetType().Name})";

        if (excludeMetadata && value is IHasMetadata)
        {
            // Write null metadata into the serialized output without mutating the returned object.
            var node = JsonSerializer.SerializeToNode(value, value.GetType(), options);

            if (node is JsonObject jsonObject)
            {
                var metadataPropertyName = options.PropertyNamingPolicy?.ConvertName(nameof(IHasMetadata.Metadatas)) ?? nameof(IHasMetadata.Metadatas);

                if (jsonObject.ContainsKey(metadataPropertyName))
                    jsonObject[metadataPropertyName] = null;
            }

            return node?.ToJsonString(options);
        }

        return JsonSerializer.Serialize(value, value.GetType(), options);
    }

    private static object ToLoggableValue(object value)
        => value != null && IsNotSerializable(value) ? $"{_notSerializedPlaceholder} ({value.GetType().Name})" : value;

    private static bool IsNotSerializable(object value)
        => value is IQueryable or Stream or Delegate or Expression or Task
           || IsAsyncEnumerable(value.GetType());

    private static bool IsAsyncEnumerable(Type type)
        => Array.Exists(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));

    /// <summary>
    /// Serializes an exception into a stable, always serializable shape.
    /// </summary>
    private static string SerializeException(Exception exception)
        => exception == null ? null : JsonSerializer.Serialize(ToLogModel(exception, depth: 0));

    private static ExceptionLogModel ToLogModel(Exception exception, int depth)
    {
        if (exception == null || depth > 5)
            return null;

        return new ExceptionLogModel
        {
            Type = exception.GetType().FullName,
            Message = exception.Message,
            Source = exception.Source,
            HResult = exception.HResult,
            StackTrace = exception.StackTrace,
            InnerException = ToLogModel(exception.InnerException, depth + 1)
        };
    }

    private sealed class ExceptionLogModel
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public int HResult { get; set; }
        public string StackTrace { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ExceptionLogModel InnerException { get; set; }
    }

    #region Masking

    private static JsonSerializerOptions _maskingSerializerOptions;
    private static JsonSerializerOptions _maskingSerializerBaseOptions;
    private static readonly Lock _maskingSerializerOptionsLock = new();
    private static readonly ConcurrentDictionary<Type, ConstructorInfo> _maskConverterConstructors = new();

    /// <summary>
    /// Builds (and caches) a <see cref="JsonSerializerOptions"/> based on the library's current options
    /// with an added type-info modifier that masks properties marked with <see cref="LogMaskedAttribute"/>.
    /// The instance is rebuilt only when the underlying <see cref="MilvaJsonConverterOptions.Current"/>
    /// changes (i.e. reconfigured at startup). Masking is applied only to log serialization; the library's
    /// own serialization options are left untouched.
    /// </summary>
    private static JsonSerializerOptions GetMaskingSerializerOptions()
    {
        var current = MilvaJsonConverterOptions.Current;

        if (_maskingSerializerOptions != null && ReferenceEquals(_maskingSerializerBaseOptions, current))
            return _maskingSerializerOptions;

        lock (_maskingSerializerOptionsLock)
        {
            if (_maskingSerializerOptions != null && ReferenceEquals(_maskingSerializerBaseOptions, current))
                return _maskingSerializerOptions;

            var options = new JsonSerializerOptions(current)
            {
                TypeInfoResolver = (current.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver()).WithAddedModifier(ApplyLogMasking),
                ReferenceHandler = current.ReferenceHandler ?? ReferenceHandler.IgnoreCycles,
            };

            _maskingSerializerBaseOptions = current;
            _maskingSerializerOptions = options;

            return options;
        }
    }

    /// <summary>
    /// Type-info modifier that makes every property marked with <see cref="LogMaskedAttribute"/> (also when the attribute is declared on an overridden base property)
    /// serialize as the mask placeholder. Properties of any type are masked; null values stay null.
    /// </summary>
    /// <param name="typeInfo">The type metadata being customized.</param>
    private static void ApplyLogMasking(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (var property in typeInfo.Properties)
        {
            var maskAttribute = property.AttributeProvider switch
            {
                PropertyInfo propertyInfo => (LogMaskedAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(LogMaskedAttribute), inherit: true),
                MemberInfo memberInfo => (LogMaskedAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(LogMaskedAttribute), inherit: true),
                _ => null
            };

            if (maskAttribute == null)
                continue;

            var mask = maskAttribute.Mask;

            if (property.PropertyType == typeof(string))
            {
                var originalGetter = property.Get;

                if (originalGetter != null)
                    property.Get = obj => originalGetter(obj) is null ? null : mask;

                continue;
            }

            var constructor = _maskConverterConstructors.GetOrAdd(property.PropertyType,
                                                                   static type => typeof(MaskingJsonConverter<>).MakeGenericType(type).GetConstructor([typeof(string)]));

            property.CustomConverter = (JsonConverter)constructor.Invoke([mask]);
        }
    }

    /// <summary>
    /// Writes the mask placeholder instead of the value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mask"></param>
    private sealed class MaskingJsonConverter<T>(string mask) : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotSupportedException();

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => writer.WriteStringValue(mask);
    }

    #endregion
}
