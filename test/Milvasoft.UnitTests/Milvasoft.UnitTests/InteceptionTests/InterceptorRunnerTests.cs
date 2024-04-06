using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvasoft.Interception.Interceptors.Logging;
using Milvasoft.Interception.Interceptors.Runner;
using System.Text.Json;

namespace Milvasoft.UnitTests.InteceptionTests;

public class InterceptorRunnerTests
{
    [Fact]
    public void Method_WithActivityAndLogInterceptor_ShouldLogCorrectly()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<ISomeInterface>();

        // Act & Assert
        sut.Invoking(x =>
        {
            var result = x.Method();

            var logs = TestLogger.Logs.Where(i => i.Value.TransactionId == result);

            logs.Should().HaveCount(2);
            logs.Should().AllSatisfy(i => i.Value.IsSuccess.Should().BeTrue());
            logs.FirstOrDefault(i => i.Value.MethodName == "MethodShouldRunWithRunner").Should().NotBeNull();

        }).Should().NotThrow();
    }

    [Fact]
    public void AnotherMethod_WithLogInterceptor_ShouldLogCorrectly()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<ISomeInterface>();

        // Act & Assert
        sut.Invoking(x =>
        {
            _ = x.AnotherMethod();

            TestLogger.Logs.Should().HaveCount(2);
            var transactionIdEquality = TestLogger.Logs.First().Value.TransactionId == TestLogger.Logs.Last().Value.TransactionId;
            transactionIdEquality.Should().BeFalse();

        }).Should().NotThrow();
    }

    #region Setup

    public interface ISomeInterface : IInterceptable
    {
        string Method();
        string AnotherMethod();
        string MethodShouldRunWithRunner(int x);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4144:Methods should not have identical implementations", Justification = "<Pending>")]
    public class SomeClass(IInterceptorRunner interceptorRunner) : ISomeInterface
    {
        private readonly IInterceptorRunner _interceptorRunner = interceptorRunner;

        [ActivityStarter]
        [Log]
        public virtual string Method()
        {
            _interceptorRunner.InterceptWithLog(() => MethodShouldRunWithRunner(3));

            return ActivityHelper.TraceId;
        }

        [Log]
        public virtual string AnotherMethod()
        {
            _interceptorRunner.InterceptWithLog(() => MethodShouldRunWithRunner(3));

            return ActivityHelper.TraceId;
        }

        public virtual string MethodShouldRunWithRunner(int x) => $"Runner result with parameter {x}";
    }

    public class TestLogEntity
    {
        public string TransactionId { get; set; }
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public string MethodParams { get; set; }
        public string MethodResult { get; set; }
        public int ElapsedMs { get; set; }
        public DateTime UtcLogTime { get; set; }
        public string CacheInfo { get; set; }
        public string Exception { get; set; }
        public bool IsSuccess { get; set; }
        public string ExtraProp { get; set; }
    }

    public class TestLogger : IMilvaLogger
    {
        public static Dictionary<int, TestLogEntity> Logs { get; set; } = [];

        public void Log(string logEntry)
        {
            var logObject = JsonSerializer.Deserialize<TestLogEntity>(logEntry);

            Logs.Add(Logs.Count + 1, logObject);
        }

        public Task LogAsync(string logEntry) => throw new NotImplementedException();
        public void Debug(string message) => throw new NotImplementedException();
        public void Debug(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Error(string message) => throw new NotImplementedException();
        public void Error(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Error(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Fatal(string message) => throw new NotImplementedException();
        public void Fatal(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Information(string message) => throw new NotImplementedException();
        public void Information(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Information(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Verbose(string message) => throw new NotImplementedException();
        public void Verbose(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Warning(string message) => throw new NotImplementedException();
        public void Warning(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddSingleton<IMilvaLogger, TestLogger>();
        builder.Services.AddScoped<ISomeInterface, SomeClass>();

        builder.Services.AddMilvaInterception([typeof(ISomeInterface)])
                        .WithActivityInterceptor()
                        .WithLogInterceptor(opt =>
                        {
                            opt.AsyncLogging = false;
                        });

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
