using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.Logging;

namespace Milvasoft.UnitTests.InteceptionTests.Fixtures;

public class CustomInterceptorAttribute : DecorateAttribute
{

    public CustomInterceptorAttribute() : base(typeof(CustomInterceptorFixture))
    {

    }
}

public class CustomInterceptorFixture(IServiceProvider serviceProvider) : IMilvaInterceptor
{
    private readonly ICustomInterceptionOptionsFixture _options = serviceProvider.GetService<ICustomInterceptionOptionsFixture>();

    public int InterceptionOrder { get; set; }

    public async Task OnInvoke(Call call)
    {
        await call.NextAsync();

        if (_options != null)
            call.ReturnValue = _options.SomeOptions;
        else
            call.ReturnValue = "intercepted";
    }
}

public class CustomInterceptionOptionsFixture : ICustomInterceptionOptionsFixture
{
    public ServiceLifetime InterceptorLifetime { get; set; } = ServiceLifetime.Scoped;
    public string SomeOptions { get; set; }
}

public interface ICustomInterceptionOptionsFixture : IInterceptionOptions
{
    public string SomeOptions { get; set; }
}
