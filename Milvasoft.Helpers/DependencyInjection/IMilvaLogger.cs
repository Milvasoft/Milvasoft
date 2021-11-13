using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Helpers.Enums;
using System;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DependencyInjection;

/// <summary>
/// Logger interface for DI. If you want to get logs from library side errors implement this interface and register to <see cref="IServiceCollection"/>.
/// </summary>
public interface IMilvaLogger
{
    /// <summary>
    /// Saves the log at verbose level.
    /// </summary>
    void LogVerbose(string message);

    /// <summary>
    /// Saves the log at info level.
    /// </summary>
    void LogInfo(string message);

    /// <summary>
    /// Saves the log at warn level.
    /// </summary>
    void LogWarning(string message);

    /// <summary>
    /// Saves the log at debug level.
    /// </summary>
    void LogDebug(string message);

    /// <summary>
    /// Saves the log at error level.
    /// </summary>
    void LogError(string message);

    /// <summary>
    /// Saves the log at fatal level.
    /// </summary>
    /// <param name="message"></param>
    void LogFatal(string message);

    /// <summary>
    /// Saves the log at fatal level. And sends mail to producer.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="mailSubject"></param>
    void LogFatal(string message, string mailSubject);

    /// <summary>
    /// Saves the log at fatal level. And sends mail to producer.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="mailSubject"></param>
    Task LogFatalAsync(string message, string mailSubject);

    /// <summary>
    /// Saves the log at fatal level. And sends mail to producer.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="mailSubject"></param>
    void LogFatal(string message, MailSubject mailSubject);

    /// <summary>
    /// Saves the log at fatal level. And sends mail to producer.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="mailSubject"></param>
    Task LogFatalAsync(string message, MailSubject mailSubject);

    #region Compatible With SeriLog

    /// <summary>
    /// Write a log event with the specified level.
    /// </summary>
    /// <param name="seriLogEventLevel"></param>
    /// <param name="messageTemplate"></param>
    void Write(SeriLogEventLevel seriLogEventLevel, string messageTemplate);

    /// <summary>
    /// Write a log event with the specified level.
    /// </summary>
    /// <param name="seriLogEventLevel"></param>
    /// <param name="exception"></param>
    /// <param name="messageTemplate"></param>
    void Write(SeriLogEventLevel seriLogEventLevel, Exception exception, string messageTemplate);

    /// <summary>
    /// Write a log event with the specified level.
    /// </summary>
    /// <param name="seriLogEventLevel"></param>
    /// <param name="messageTemplate"></param>
    /// <param name="propertyValues"></param>
    void Write(SeriLogEventLevel seriLogEventLevel, string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Write a log event with the specified level and associated exception.
    /// </summary>
    /// <param name="seriLogEventLevel"></param>
    /// <param name="exception"></param>
    /// <param name="messageTemplate"></param>
    /// <param name="propertyValues"></param>
    void Write(SeriLogEventLevel seriLogEventLevel, Exception exception, string messageTemplate, params object[] propertyValues);

    #endregion
}

/// <summary>
/// Specifies the meaning and relative importance of a log event.
/// </summary>
public enum SeriLogEventLevel
{
    /// <summary>
    /// Anything and everything you might want to know about a running block of code.
    /// </summary>
    Verbose = 0,
    /// <summary>
    /// Internal system events that aren't necessarily observable from the outside.
    /// </summary>
    Debug = 1,
    /// <summary>
    /// The lifeblood of operational intelligence - things happen.
    /// </summary>
    Information = 2,
    /// <summary>
    /// Service is degraded or endangered.
    /// </summary>
    Warning = 3,
    /// <summary>
    /// Functionality is unavailable, invariants are broken or data is lost.
    /// </summary>
    Error = 4,
    /// <summary>
    /// If you have a pager, it goes off when one of these occurs.
    /// </summary>

    Fatal = 5
}
