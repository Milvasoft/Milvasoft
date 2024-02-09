using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.ActivityScope;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Milvasoft.Interception.Interceptors.Logging;

public class LogInterceptor : IMilvaInterceptor
{
    public int InterceptionOrder { get; set; } = -1;

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

        stopwatch.Start();

        await call.NextAsync();

        stopwatch.Stop();

        var logObjectPropDic = _logInterceptionOptions.LogDefaultParameters ? new Dictionary<string, object>()
        {
            { "TransactionId", ActivityHelper.TraceId },
            { "MethodName", call.Method.Name },
            { "MethodParams", call.Arguments.ToJson() },
            { "MethodResult", call.ReturnValue.ToJson() },
            { "ElapsedMs", stopwatch.ElapsedMilliseconds },
            { "UtcLogTime" , DateTime.UtcNow },
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

        if (_logInterceptionOptions.LogAsync)
        {
            await _logger.LogAsync(logObjectPropDic.ToJson());
        }
        else _logger.Log(logObjectPropDic.ToJson());
    }

}