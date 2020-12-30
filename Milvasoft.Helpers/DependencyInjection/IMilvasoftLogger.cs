using Milvasoft.Helpers.Enums;

namespace Milvasoft.DependencyInjection
{
    /// <summary>
    /// Logger interface for DI.
    /// </summary>
    public interface IMilvasoftLogger
    {
        /// <summary>
        /// <para><b>EN: </b>Saves the log at info level.</para>
        /// <para><b>TR: </b>Log kaydını info seviyesinde kaydeder.</para>
        /// </summary>
        void LogInfo(string message);

        /// <summary>
        /// <para><b>EN: </b>Saves the log at warn level.</para>
        /// <para><b>TR: </b>Log kaydını debug warn kaydeder.</para>s
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// <para><b>EN: </b>Saves the log at debug level.</para>
        /// <para><b>TR: </b>Log kaydını debug seviyesinde kaydeder.</para>
        /// </summary>
        void LogDebug(string message);

        /// <summary>
        /// <para><b>EN: </b>Saves the log at error level.</para>
        /// <para><b>TR: </b>Log kaydını error seviyesinde kaydeder.</para>
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
