using Milvasoft.Core.Abstractions;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Milvasoft.UnitTests.InteceptionTests.Fixtures;

/// <summary>
/// Logger that keeps log entries per instance (register as singleton per service provider).
/// </summary>
public class RecordingLoggerFixture : IMilvaLogger
{
    public ConcurrentQueue<string> Entries { get; } = new();

    public bool ThrowOnLog { get; set; }

    public JsonElement Last => JsonDocument.Parse(Entries.Last()).RootElement;

    public void Log(string logEntry)
    {
        if (ThrowOnLog)
            throw new InvalidOperationException("Logger failure");

        Entries.Enqueue(logEntry);
    }

    public Task LogAsync(string logEntry)
    {
        Log(logEntry);
        return Task.CompletedTask;
    }

    public void Debug(string message) { }
    public void Debug(string messageTemplate, params object[] propertyValues) { }
    public void Debug(Exception exception, string messageTemplate) { }
    public void Debug(Exception exception, string messageTemplate, params object[] propertyValues) { }
    public void Error(string message) { }
    public void Error(string messageTemplate, params object[] propertyValues) { }
    public void Error(Exception exception, string messageTemplate) { }
    public void Error(Exception exception, string messageTemplate, params object[] propertyValues) { }
    public void Fatal(string message) { }
    public void Fatal(string messageTemplate, params object[] propertyValues) { }
    public void Fatal(Exception exception, string messageTemplate) { }
    public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues) { }
    public void Information(string message) { }
    public void Information(string messageTemplate, params object[] propertyValues) { }
    public void Information(Exception exception, string messageTemplate) { }
    public void Information(Exception exception, string messageTemplate, params object[] propertyValues) { }
    public void Verbose(string message) { }
    public void Verbose(string messageTemplate, params object[] propertyValues) { }
    public void Verbose(Exception exception, string messageTemplate) { }
    public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues) { }
    public void Warning(string message) { }
    public void Warning(string messageTemplate, params object[] propertyValues) { }
    public void Warning(Exception exception, string messageTemplate) { }
    public void Warning(Exception exception, string messageTemplate, params object[] propertyValues) { }
}
