using Milvasoft.Interception.Decorator;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests.Common;

public class TestDecorator : IMilvaInterceptor
{
    public int InterceptionOrder { get; set; } = 1;
    public bool WasInvoked { get; set; }

    public Task OnInvoke(Call call)
    {
        WasInvoked = true;
        return call.NextAsync();
    }
}
