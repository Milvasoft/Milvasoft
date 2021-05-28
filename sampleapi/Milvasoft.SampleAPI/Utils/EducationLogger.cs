
using Milvasoft.Helpers.DependencyInjection;
using Milvasoft.Helpers.Enums;
using Milvasoft.Helpers.Mail;
using Serilog;
using System.Threading.Tasks;

namespace Milvasoft.SampleAPI.Utils
{
    /// <summary>
    /// <para><b>EN: </b>The education project logger class.</para>
    /// <para><b>TR: </b>Education projesi logger sınıfı.</para>
    /// </summary>
    public class EducationLogger : IMilvaLogger
    {
        private readonly ILogger _logger = Log.Logger;
        private readonly IMilvaMailSender _milvaMailSender;

        public EducationLogger(IMilvaMailSender milvaMailSender)
        {
            _milvaMailSender = milvaMailSender;
        }

        /// <summary>
        /// <para><b>EN: </b>Saves the log at debug level.</para>
        /// <para><b>TR: </b>Log kaydını debug seviyesinde kaydeder.</para>
        /// </summary>
        /// <param name="message"></param>
        public void LogVerbose(string message) => _logger.Verbose(message);

        /// <summary>
        /// <para><b>EN: </b>Saves the log at debug level.</para>
        /// <para><b>TR: </b>Log kaydını debug seviyesinde kaydeder.</para>
        /// </summary>
        /// <param name="message"></param>
        public void LogDebug(string message) => _logger.Debug(message);

        /// <summary>
        /// <para><b>EN: </b>Saves the log at error level.</para>
        /// <para><b>TR: </b>Log kaydını error seviyesinde kaydeder.</para>
        /// </summary>
        /// <param name="message"></param>
        public void LogError(string message) => _logger.Error(message);

        /// <summary>
        /// <para><b>EN: </b>Saves the log at info level.</para>
        /// <para><b>TR: </b>Log kaydını info seviyesinde kaydeder.</para>
        /// </summary>
        /// <param name="message"></param>
        public void LogInfo(string message) => _logger.Information(message);

        /// <summary>
        /// <para><b>EN: </b>Saves the log at warn level.</para>
        /// <para><b>TR: </b>Log kaydını debug warn kaydeder.</para>
        /// </summary>
        /// <param name="message"></param>
        public void LogWarning(string message) => _logger.Warning(message);

        /// <summary>
        /// <para><b>EN: </b>Saves the log at fatal level.</para>
        /// <para><b>TR: </b>Log kaydını fatal seviyesinde kaydeder. </para>
        /// </summary>
        /// <param name="message"></param>
        public void LogFatal(string message)
        {
            _logger.Fatal(message);
        }

        /// <summary>
        /// <para><b>EN: </b>Saves the log at fatal level. And sends mail to producer. </para>
        /// <para><b>TR: </b>Log kaydını fatal seviyesinde kaydeder.Ve üreticiye e-mail gönderir. </para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mailSubjectsEnum"></param>
        public void LogFatal(string message, MailSubject mailSubjectsEnum)
        {
            _logger.Fatal(message);

            _milvaMailSender.MilvaSendMail("ogibaran96@gmail.com", mailSubjectsEnum, message);
        }

        /// <summary>
        /// <para><b>EN: </b>Saves the log at fatal level. And sends mail to producer. </para>
        /// <para><b>TR: </b>Log kaydını fatal seviyesinde kaydeder.Ve üreticiye e-mail gönderir. </para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mailSubjectsEnum"></param>
        public async Task LogFatalAsync(string message, MailSubject mailSubjectsEnum)
        {
            _logger.Fatal(message);

            await _milvaMailSender.MilvaSendMailAsync("ogibaran96@gmail.com", mailSubjectsEnum, message).ConfigureAwait(false);
        }

        /// <summary>
        /// <para><b>EN: </b>Saves the log at fatal level. And sends mail to producer. </para>
        /// <para><b>TR: </b>Log kaydını fatal seviyesinde kaydeder.Ve üreticiye e-mail gönderir. </para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mailSubjectsEnum"></param>
        public void LogFatal(string message, string mailSubjectsEnum)
        {
            _logger.Fatal(message);

            _milvaMailSender.MilvaSendMail("ogibaran96@gmail.com", mailSubjectsEnum, message);

        }

        /// <summary>
        /// <para><b>EN: </b>Saves the log at fatal level. And sends mail to producer. </para>
        /// <para><b>TR: </b>Log kaydını fatal seviyesinde kaydeder.Ve üreticiye e-mail gönderir. </para>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mailSubjectsEnum"></param>
        public async Task LogFatalAsync(string message, string mailSubjectsEnum)
        {
            _logger.Fatal(message);

            await _milvaMailSender.MilvaSendMailAsync("ogibaran96@gmail.com", mailSubjectsEnum, message).ConfigureAwait(false);
        }
    }
}
