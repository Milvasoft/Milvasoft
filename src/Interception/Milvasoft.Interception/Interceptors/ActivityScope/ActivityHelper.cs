using System.Diagnostics;

namespace Milvasoft.Interception.Interceptors.ActivityScope;

/// <summary>
/// Helper class for retrieving information about the current activity.
/// </summary>
public static class ActivityHelper
{
    /// <summary>
    /// Id of current activity.
    /// </summary>
    public static string Id => Activity.Current?.Id ?? "No Activity";

    /// <summary>
    /// Span id of current activity.
    /// </summary>
    public static string SpanId => Activity.Current?.SpanId.ToString() ?? "No Span";

    /// <summary>
    /// Parent id of current activity.
    /// </summary>
    public static string Parent => Activity.Current?.ParentId ?? "No Parent";

    /// <summary>
    /// Trace id of current activity. If no activity occurs creates random acitivity id.
    /// </summary>
    public static string TraceId => Activity.Current?.TraceId.ToString() ?? ActivityTraceId.CreateRandom().ToString();

    /// <summary>
    /// Duration of current activity.
    /// </summary>
    public static string Duration => Activity.Current?.Duration.TotalMilliseconds.ToString() ?? "No Duration";

    /// <summary>
    /// Operation name of current activity.
    /// </summary>
    public static string OperationName => Activity.Current?.OperationName ?? "No Activity";
}
