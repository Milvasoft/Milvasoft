using Milvasoft.Interception.Decorator;
using System.Diagnostics;

namespace Milvasoft.Interception.Interceptors.ActivityScope;

/// <summary>
/// Starts an activity to associate interrelated calls made from different methods. Details of this activity can be accessed using the <see cref="ActivityHelper"/> class.
/// </summary>
public class ActivityInterceptor : IMilvaInterceptor
{
    /// <inheritdoc/>
    public static int InterceptionOrder { get; set; } = int.MinValue;

    public async Task OnInvoke(Call call)
    {
        var activityAttribute = call.GetInterceptorAttribute<ActivityStarterAttribute>();

        var activity = new Activity(activityAttribute.Name);

        activity.Start();

        await call.NextAsync();

        activity.Dispose();
    }
}