using Microsoft.Extensions.DependencyInjection;

namespace Milvasoft.Core.Abstractions;

/// <summary>
/// Logger interface for DI. If you want to get logs from library side errors implement this interface and register to <see cref="IServiceCollection"/>.
/// </summary>
public interface IMilvaLogger
{
    /// <summary>
    /// Abstraction where you can make logs such as database logging.
    /// </summary>
    /// <param name="logEntry">Log object json string.</param>
    public void Log(string logEntry);

    /// <summary>
    /// Abstraction where you can make logs such as database logging.
    /// </summary>
    /// <param name="logEntry">Log object json string</param>
    public Task LogAsync(string logEntry);

    /// <summary>
    /// Write a log event with verbose level.
    /// </summary>
    /// <param name="message">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    public void Verbose(string message);

    /// <summary>
    /// Write a log event with the verbose level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose(exception, "Staring into space, wondering where this comet came from.");
    /// </example>
    public void Verbose(Exception exception, string messageTemplate);

    /// <summary>
    /// Write a log event with the verbose level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into {@from}, wondering where this comet came from.", space);
    /// </example>
    public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Write a log event with debug level.
    /// </summary>
    /// <param name="message">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    public void Debug(string message);

    /// <summary>
    /// Write a log event with the debug level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose(exception, "Staring into space, wondering where this comet came from.");
    /// </example>
    public void Debug(Exception exception, string messageTemplate);

    /// <summary>
    /// Write a log event with the debug level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into {@from}, wondering where this comet came from.", space);
    /// </example>
    public void Debug(Exception exception, string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Write a log event with information level.
    /// </summary>
    /// <param name="message">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    public void Information(string message);

    /// <summary>
    /// Write a log event with the information level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose(exception, "Staring into space, wondering where this comet came from.");
    /// </example>
    public void Information(Exception exception, string messageTemplate);

    /// <summary>
    /// Write a log event with the information level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into {@from}, wondering where this comet came from.", space);
    /// </example>
    public void Information(Exception exception, string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Write a log event with warning level.
    /// </summary>
    /// <param name="message">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    public void Warning(string message);

    /// <summary>
    /// Write a log event with the warning level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose(exception, "Staring into space, wondering where this comet came from.");
    /// </example>
    public void Warning(Exception exception, string messageTemplate);

    /// <summary>
    /// Write a log event with the warning level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into {@from}, wondering where this comet came from.", space);
    /// </example>
    public void Warning(Exception exception, string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Write a log event with error level.
    /// </summary>
    /// <param name="message">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    public void Error(string message);

    /// <summary>
    /// Write a log event with the error level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose(exception, "Staring into space, wondering where this comet came from.");
    /// </example>
    public void Error(Exception exception, string messageTemplate);

    /// <summary>
    /// Write a log event with the error level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into {@from}, wondering where this comet came from.", space);
    /// </example>
    public void Error(Exception exception, string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Write a log event with fatal level.
    /// </summary>
    /// <param name="message">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose("Staring into space, wondering if we're alone.");
    /// </example>
    public void Fatal(string message);

    /// <summary>
    /// Write a log event with the fatal level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <example>
    /// Log.Verbose(exception, "Staring into space, wondering where this comet came from.");
    /// </example>
    public void Fatal(Exception exception, string messageTemplate);

    /// <summary>
    /// Write a log event with the fatal level and associated exception.
    /// </summary>
    /// <param name="exception">Exception related to the event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="propertyValues">Objects positionally formatted into the message template.</param>
    /// <example>
    /// Log.Verbose(ex, "Staring into {@from}, wondering where this comet came from.", space);
    /// </example>
    public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues);
}