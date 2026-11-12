using Milvasoft.Interception.Decorator;
using System.Diagnostics;

namespace Milvasoft.Interception.Interceptors.ActivityScope;

/// <summary>
/// Starts an activity to associate interrelated calls made from different methods. Details of this activity can be accessed using the <see cref="ActivityHelper"/> class.
/// </summary>
/// <remarks>
/// Activities are created from <see cref="Source"/> so listeners (for example OpenTelemetry) can sample and export them.
/// When nothing listens to the source, a plain <see cref="Activity"/> is started so the trace id is still shared by the calls inside the method.
/// The activity is stopped when the method's work completes, also when it fails.
/// </remarks>
public class ActivityInterceptor : IMilvaInterceptor
{
    /// <summary>
    /// Source name of the activities started by this interceptor.
    /// </summary>
    public const string ActivitySourceName = "Milvasoft.Interception";

    /// <summary>
    /// Activity source used to start activities.
    /// </summary>
    public static ActivitySource Source { get; } = new(ActivitySourceName);

    /// <inheritdoc/>
    public int InterceptionOrder { get; set; } = int.MinValue;

    /// <summary>
    /// Starts a new activity.
    /// </summary>
    /// <param name="call"></param>
    /// <returns></returns>
    public async Task OnInvoke(Call call)
    {
        var activityName = call.GetInterceptorAttribute<ActivityStarterAttribute>()?.Name ?? call.Method.Name;

        using var activity = Source.StartActivity(activityName) ?? new Activity(activityName).Start();

        try
        {
            await call.NextAsync();
        }
        catch (Exception ex)
        {
            activity.SetStatus(ActivityStatusCode.Error, ex.Message);

            throw;
        }
    }
}
