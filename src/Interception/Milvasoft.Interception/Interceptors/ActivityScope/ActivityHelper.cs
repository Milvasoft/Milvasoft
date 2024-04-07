using System.Diagnostics;

namespace Milvasoft.Interception.Interceptors.ActivityScope;

/// <summary>
/// Helper class for retrieving information about the current activity.
/// </summary>
public static class ActivityHelper
{
    public static string Id => Activity.Current?.Id ?? "No Activity";
    public static string SpanId => Activity.Current?.SpanId.ToString() ?? "No Span";
    public static string Parent => Activity.Current?.ParentId ?? "No Parent";
    public static string TraceId => Activity.Current?.TraceId.ToString() ?? ActivityTraceId.CreateRandom().ToString();
    public static string Duration => Activity.Current?.Duration.TotalMilliseconds.ToString() ?? "No Duration";
    public static string OperationName => Activity.Current?.OperationName ?? "No Activity";
}
