using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Core.Abstractions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Interception.Builder;
using Milvasoft.Interception.Decorator;
using Milvasoft.Interception.Interceptors.ActivityScope;
using Milvasoft.Interception.Interceptors.Logging;
using System.Text.Json;

namespace Milvasoft.UnitTests.InteceptionTests;

[Trait("Interceptors Unit Tests", "Unit tests for Milvasoft.Interception project interceptors.")]
public class LogInterceptorTests
{
    [Fact]
    public void Method_WithActivityAndLogInterceptor_ShouldLogCorrectly()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act & Assert
        sut.Invoking(x =>
        {
            var result = x.Method();

            var logExistsWithActivityId = TestLogger.Logs.TryGetValue(result, out var logEntity);

            logExistsWithActivityId.Should().BeTrue();
            logEntity.ExtraProp.Should().Be("Extra prop");

        }).Should().NotThrow();
    }

    [Fact]
    public void MethodThrowsException_WithActivityAndLogInterceptor_ShouldLogCorrectly()
    {
        // Arrange
        var services = GetServices();
        var sut = services.GetService<SomeClass>();

        // Act & Assert
        sut.Invoking(x =>
        {
            var result = x.MethodThrowsException();

            var logExistsWithActivityId = TestLogger.Logs.TryGetValue(result, out var logEntity);

            logExistsWithActivityId.Should().BeTrue();
            logEntity.ExtraProp.Should().Be("Extra prop");
            logEntity.Exception.Should().NotBeNull();

        }).Should().Throw<MilvaDeveloperException>();
    }

    [Fact]
    public void Method_WithActivityAndLogInterceptorButLoggerIsNull_ShouldDoNothing()
    {
        // Arrange
        var services = GetServicesWithoutLogger();
        var sut = services.GetService<SomeClass>();

        // Act & Assert
        sut.Invoking(x =>
        {
            var result = x.Method();

            var logExistsWithActivityId = TestLogger.Logs.TryGetValue(result, out var _);

            logExistsWithActivityId.Should().BeFalse();

        }).Should().NotThrow();
    }

    #region Setup

    public interface ISomeClass
    {
        [ActivityStarter("LogActivity")]
        [Log]
        public string Method();

        [ActivityStarter("LogActivity")]
        [Log]
        public string MethodThrowsException();
    }

    public class SomeClass : IInterceptable
    {
        [ActivityStarter("LogActivity")]
        [Log]
        public virtual string Method() => ActivityHelper.TraceId;

        [ActivityStarter("LogActivity")]
        [Log]
        public virtual string MethodThrowsException() => throw new MilvaDeveloperException();
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
        public static Dictionary<string, TestLogEntity> Logs { get; set; } = [];

        public void Log(string logEntry)
        {
            var logObject = JsonSerializer.Deserialize<TestLogEntity>(logEntry);

            Logs.Add(logObject.TransactionId, logObject);
        }

        public async Task LogAsync(string logEntry) => await Task.Run(() => Log(logEntry));

        public void Debug(string message) => throw new NotImplementedException();
        public void Debug(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Debug(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Error(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Error(string message) => throw new NotImplementedException();
        public void Error(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Error(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Fatal(string message) => throw new NotImplementedException();
        public void Fatal(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Fatal(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Information(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Information(string message) => throw new NotImplementedException();
        public void Information(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Information(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Verbose(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Verbose(string message) => throw new NotImplementedException();
        public void Verbose(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Warning(string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
        public void Warning(string message) => throw new NotImplementedException();
        public void Warning(Exception exception, string messageTemplate) => throw new NotImplementedException();
        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues) => throw new NotImplementedException();
    }

    private static ServiceProvider GetServices()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddSingleton<IMilvaLogger, TestLogger>();
        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(SomeClass)])
                        .WithActivityInterceptor()
                        .WithLogInterceptor(opt =>
                        {
                            opt.AsyncLogging = false;
                            opt.ExtraLoggingPropertiesSelector = (sp) => new { ExtraProp = "Extra prop" };
                        });

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    private static ServiceProvider GetServicesWithoutLogger()
    {
        var builder = new InterceptionBuilder(new ServiceCollection());

        builder.Services.AddTransient<SomeClass>();

        builder.Services.AddMilvaInterception([typeof(SomeClass)])
                        .WithActivityInterceptor()
                        .WithLogInterceptor(opt =>
                        {
                            opt.AsyncLogging = true;
                            opt.ExtraLoggingPropertiesSelector = (sp) => new { ExtraProp = "Extra prop" };
                        });

        var serviceProvider = builder.Services.BuildServiceProvider();

        return serviceProvider;
    }

    #endregion
}
