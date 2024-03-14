using Milvasoft.Interception.Decorator;

namespace Milvasoft.Interception.Interceptors.ActivityScope;

public class ActivityStarterAttribute : DecorateAttribute
{
    public string Name { get; private set; }

    public ActivityStarterAttribute() : base(typeof(ActivityInterceptor))
    {
        Name = "BaseActivity";
    }
    public ActivityStarterAttribute(string name) : base(typeof(ActivityInterceptor))
    {
        Name = name;
    }
}