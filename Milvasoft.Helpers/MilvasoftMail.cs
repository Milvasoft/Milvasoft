using Milvasoft.Helpers.Enums;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace Milvasoft.Helpers
{
    /// <summary>
    /// Provides send mail.
    /// </summary>
    public class MilvasoftMail
    {
        /// <summary>
        /// Provides send mail.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="networkCredential"></param>
        /// <param name="smtpPort"></param>
        /// <param name="smtpHost"></param>
        /// <param name="body"></param>
        /// <param name="isBodyHtml"></param>
        public static void OpsiyonSendMail(string from,
                                           string to,
                                           string subject,
                                           NetworkCredential networkCredential,
                                           int smtpPort,
                                           string smtpHost,
                                           string body,
                                           bool isBodyHtml = false)
        {
            MailMessage mailMessage = new MailMessage(from, to, subject, body);
            mailMessage.IsBodyHtml = isBodyHtml;


            SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.Credentials = networkCredential;

            smtpClient.SendAsync(mailMessage, (object)mailMessage);
        }

        /// <summary>
        /// Provides send mail with attachment.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="networkCredential"></param>
        /// <param name="smtpPort"></param>
        /// <param name="smtpHost"></param>
        /// <param name="body"></param>
        /// <param name="filePath"></param>
        /// <param name="isBodyHtml"></param>
        public static void OpsiyonSendMail(string from,
                                           string to,
                                           string subject,
                                           NetworkCredential networkCredential,
                                           int smtpPort,
                                           string smtpHost,
                                           string body,
                                           string filePath,
                                           bool isBodyHtml = false)
        {
            MailMessage mailMessage = new MailMessage(from, to, subject, body);
            mailMessage.IsBodyHtml = isBodyHtml;

            // Create  the file attachment for this email message.
            Attachment data = new Attachment(filePath, MediaTypeNames.Application.Octet);
            // Add time stamp information for the file.
            ContentDisposition disposition = data.ContentDisposition;
            disposition.CreationDate = System.IO.File.GetCreationTime(filePath);
            disposition.ModificationDate = System.IO.File.GetLastWriteTime(filePath);
            disposition.ReadDate = System.IO.File.GetLastAccessTime(filePath);
            // Add the file attachment to this email message.
            mailMessage.Attachments.Add(data);

            SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.Credentials = networkCredential;

            smtpClient.SendAsync(mailMessage, (object)mailMessage);
        }

        /// <summary>
        /// Provides send mail.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="networkCredential"></param>
        /// <param name="smtpPort"></param>
        /// <param name="smtpHost"></param>
        /// <param name="body"></param>
        /// <param name="isBodyHtml"></param>
        public static void OpsiyonSendMail(string from,
                                           string to,
                                           MailSubject subject,
                                           NetworkCredential networkCredential,
                                           int smtpPort,
                                           string smtpHost,
                                           string body,
                                           bool isBodyHtml = false)
        {
            MailMessage mailMessage = new MailMessage(from, to, CommonHelper.GetEnumDesciption(subject), body);
            mailMessage.IsBodyHtml = isBodyHtml;


            SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.Credentials = networkCredential;

            smtpClient.SendAsync(mailMessage, (object)mailMessage);
        }
    }
}
