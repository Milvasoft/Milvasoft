﻿using Milvasoft.Helpers.Enums;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.DependencyInjection
{
    /// <summary>
    /// Logger interface for DI.
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
    }
}
