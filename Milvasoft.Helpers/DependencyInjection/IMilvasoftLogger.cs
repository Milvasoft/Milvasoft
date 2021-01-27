using Milvasoft.Helpers.Enums;

namespace Milvasoft.DependencyInjection
{
    /// <summary>
    /// Logger interface for DI.
    /// </summary>
    public interface IMilvasoftLogger
    {
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
        void LogFatal(string message, MailSubject mailSubject);
    }
}
