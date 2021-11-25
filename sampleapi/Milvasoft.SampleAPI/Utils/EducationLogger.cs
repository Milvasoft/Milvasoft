
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.Enums;
using Milvasoft.Helpers.Mail;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Utils
{
    /// <summary>
    /// The education project logger class.
    /// </summary>
    public class EducationLogger : IMilvaLogger
    {
        private readonly ILogger _logger = Log.Logger;
        private readonly IMilvaMailSender _milvaMailSender;

        /// <summary>
        /// Initializes new instance of <see cref="EducationLogger"/>
        /// </summary>
        /// <param name="milvaMailSender"></param>
        public EducationLogger(IMilvaMailSender milvaMailSender)
        {
            _milvaMailSender = milvaMailSender;
        }

        /// <summary>
        /// Saves the log at debug level.
        /// </summary>
        /// <param name="message"></param>
        public void LogVerbose(string message) => _logger.Verbose(message);

        /// <summary>
        /// Saves the log at debug level.
        /// </summary>
        /// <param name="message"></param>
        public void LogDebug(string message) => _logger.Debug(message);

        /// <summary>
        /// Saves the log at error level.
        /// </summary>
        /// <param name="message"></param>
        public void LogError(string message) => _logger.Error(message);

        /// <summary>
        /// Saves the log at info level.
        /// </summary>
        /// <param name="message"></param>
        public void LogInfo(string message) => _logger.Information(message);

        /// <summary>
        /// Saves the log at warn level.
        /// </summary>
        /// <param name="message"></param>
        public void LogWarning(string message) => _logger.Warning(message);

        /// <summary>
        /// Saves the log at fatal level.
        /// </summary>
        /// <param name="message"></param>
        public void LogFatal(string message)
        {
            _logger.Fatal(message);
        }

        /// <summary>
        /// Saves the log at fatal level. And sends mail to producer.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mailSubjectsEnum"></param>
        public void LogFatal(string message, MailSubject mailSubjectsEnum)
        {
            _logger.Fatal(message);

            _milvaMailSender.MilvaSendMail("ogibaran96@gmail.com", mailSubjectsEnum, message);
        }

        /// <summary>
        /// Saves the log at fatal level. And sends mail to producer.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mailSubjectsEnum"></param>
        public async Task LogFatalAsync(string message, MailSubject mailSubjectsEnum)
        {
            _logger.Fatal(message);

            await _milvaMailSender.MilvaSendMailAsync("ogibaran96@gmail.com", mailSubjectsEnum, message).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves the log at fatal level. And sends mail to producer.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mailSubjectsEnum"></param>
        public void LogFatal(string message, string mailSubjectsEnum)
        {
            _logger.Fatal(message);

            _milvaMailSender.MilvaSendMail("ogibaran96@gmail.com", mailSubjectsEnum, message);

        }

        /// <summary>
        /// Saves the log at fatal level. And sends mail to producer.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mailSubjectsEnum"></param>
        public async Task LogFatalAsync(string message, string mailSubjectsEnum)
        {
            _logger.Fatal(message);

            await _milvaMailSender.MilvaSendMailAsync("ogibaran96@gmail.com", mailSubjectsEnum, message).ConfigureAwait(false);
        }

        /// <summary>
        /// Write a log event with the specified level.
        /// </summary>
        /// <param name="seriLogEventLevel"></param>
        /// <param name="messageTemplate"></param>
        public void Write(SeriLogEventLevel seriLogEventLevel, string messageTemplate)
            => _logger.Write((LogEventLevel)seriLogEventLevel, messageTemplate);

        /// <summary>
        /// Write a log event with the specified level.
        /// </summary>
        /// <param name="seriLogEventLevel"></param>
        /// <param name="exception"></param>
        /// <param name="messageTemplate"></param>
        public void Write(SeriLogEventLevel seriLogEventLevel, Exception exception, string messageTemplate)
            => _logger.Write((LogEventLevel)seriLogEventLevel, exception, messageTemplate);

        /// <summary>
        /// Write a log event with the specified level.
        /// </summary>
        /// <param name="seriLogEventLevel"></param>
        /// <param name="messageTemplate"></param>
        /// <param name="propertyValues"></param>
        public void Write(SeriLogEventLevel seriLogEventLevel, string messageTemplate, params object[] propertyValues)
            => _logger.Write((LogEventLevel)seriLogEventLevel, messageTemplate, propertyValues);

        /// <summary>
        /// Write a log event with the specified level and associated exception.
        /// </summary>
        /// <param name="seriLogEventLevel"></param>
        /// <param name="exception"></param>
        /// <param name="messageTemplate"></param>
        /// <param name="propertyValues"></param>
        public void Write(SeriLogEventLevel seriLogEventLevel, Exception exception, string messageTemplate, params object[] propertyValues)
            => _logger.Write((LogEventLevel)seriLogEventLevel, exception, messageTemplate, propertyValues);

    }
}
