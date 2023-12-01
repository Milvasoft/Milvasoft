using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.ActivityScope;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;

namespace Milvasoft.Interception.Interceptors.Logging;

public class LogInterceptor : IMilvaInterceptor
{
    /// TODO : ILOggerla uyumlu çalışacak bir yapı geliştir.  

    public int InterceptionOrder { get; set; } = -1;

    public async Task OnInvoke(Call call)
    {
        var logAttribute = call.GetInterceptorAttribute<LogAttribute>();

        var defaultLog = logAttribute != null;

        var stopwatch = new Stopwatch();

        stopwatch.Start();

        await call.NextAsync();

        stopwatch.Stop();

        var ms = stopwatch.ElapsedMilliseconds;

        if (defaultLog)
        {
            var log = new LogObject
            {
                TransactionId = ActivityHelper.TraceId,
                Id = ActivityHelper.Id,
                SpanId = ActivityHelper.SpanId,
                Parent = ActivityHelper.Parent,
                TraceId = ActivityHelper.TraceId,
                MethodName = call.Method.Name,
                MethodParams = JsonSerializer.Serialize(call.Arguments),
                MethodResult = JsonSerializer.Serialize(call.ReturnValue),
                ElapsedMs = ms.ToString()
            };

            Console.WriteLine(log);
            Console.WriteLine("\n");
        }
        else
        {
            var expression = call.Arguments[0];

            var body = (MethodCallExpression)expression.GetType().GetProperty("Body").GetValue(expression);

            var values = new List<object>();

            foreach (var argument in body.Arguments)
            {
                values.Add(argument.GetType().GetProperty("Value").GetValue(argument));
            }

            var argumentValues = values;

            var methodName = body.Method.Name;

            var log = new LogObject
            {
                TransactionId = ActivityHelper.TraceId,
                Id = ActivityHelper.Id,
                SpanId = ActivityHelper.SpanId,
                Parent = ActivityHelper.Parent,
                TraceId = ActivityHelper.TraceId,
                MethodName = methodName.ToString(),
                MethodParams = JsonSerializer.Serialize(argumentValues),
                MethodResult = JsonSerializer.Serialize(call.ReturnValue),
                ElapsedMs = ms.ToString()
            };

            Console.WriteLine(log);
            Console.WriteLine("\n");
        }
    }

    public static MemberExpression ResolveMemberExpression(Expression expression)
    {

        if (expression is MemberExpression)
        {
            return (MemberExpression)expression;
        }
        else if (expression is UnaryExpression)
        {
            // if casting is involved, Expression is not x => x.FieldName but x => Convert(x.Fieldname)
            return (MemberExpression)((UnaryExpression)expression).Operand;
        }
        else
        {
            throw new NotSupportedException(expression.ToString());
        }
    }

    private static object GetValue(MemberExpression exp)
    {
        // expression is ConstantExpression or FieldExpression
        if (exp.Expression is ConstantExpression)
        {
            return ((ConstantExpression)exp.Expression).Value
                    .GetType()
                    .GetField(exp.Member.Name)
                    .GetValue(((ConstantExpression)exp.Expression).Value);
        }
        else if (exp.Expression is MemberExpression)
        {
            return GetValue((MemberExpression)exp.Expression);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    static string MethodNameFor<T>(Expression<Func<T>> expression)
    {
        return ((MethodCallExpression)expression.Body).Method.Name;
    }

}

public record LogObject
{
    public string TransactionId { get; set; }
    public string Id { get; set; }
    public string SpanId { get; set; }
    public string Parent { get; set; }
    public string TraceId { get; set; }
    public string MethodName { get; set; }
    public string MethodParams { get; set; }
    public string MethodResult { get; set; }
    public string ElapsedMs { get; set; }
}