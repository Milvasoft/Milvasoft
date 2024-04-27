using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.ActivityScope;

/// <summary>
/// Ensures that the activit accessed within the method marked with this attribute remains the same.
/// </summary>
public class ActivityStarterAttribute : DecorateAttribute
{
    private const string _defaultActivityName = "BaseActivity";

    /// <summary>
    /// Activiy name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Marks method with 'BaseActivity'.
    /// </summary>
    public ActivityStarterAttribute() : base(typeof(ActivityInterceptor))
    {
        Name = _defaultActivityName;
    }

    /// <summary>
    /// Marks method with <paramref name="name"/>
    /// </summary>
    /// <param name="name"></param>
    public ActivityStarterAttribute(string name) : base(typeof(ActivityInterceptor))
    {
        Name = name;
    }
}