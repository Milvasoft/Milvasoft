using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;
using Milvasoft.UnitTests.InteceptionTests.DecoratorTests.Common;
using System.Net;

namespace Milvasoft.UnitTests.InteceptionTests.DecoratorTests;

public class HttpClientTests
{
    [Fact]
    public async Task I_can_decorate_typed_http_client_that_was_registered_with_interface_and_implementation_type()
    {
        // Arrange
        var builder = new InterceptionBuilder(new ServiceCollection());
        builder.Services.AddHttpClient<ITypedHttpClient, TypedHttpClient>(client => client.BaseAddress = new Uri("https://www.microsoft.com/"));
        builder.Services.AddScoped<TestDecorator>(); // It's a singleton for testing purposes only, use transient in real code.
        builder.Intercept(typeof(ITypedHttpClient));

        var serviceProvider = builder.Services.BuildServiceProvider();

        var sut = serviceProvider.GetRequiredService<ITypedHttpClient>();

        var decorator = serviceProvider.GetRequiredService<TestDecorator>();

        // Act
        var actual = await sut.Post();

        // Assert
        actual.StatusCode.Should().Be(HttpStatusCode.OK);
        decorator.WasInvoked.Should().BeTrue();
    }

    #region Setup
    public interface ITypedHttpClient : IInterceptable
    {
        Task<HttpResponseMessage> Post();
    }

    public class TypedHttpClient : ITypedHttpClient
    {
        public TypedHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        [Decorate(typeof(TestDecorator))]
        public async Task<HttpResponseMessage> Post() => await HttpClient.GetAsync("windows");
    }
    #endregion
}
