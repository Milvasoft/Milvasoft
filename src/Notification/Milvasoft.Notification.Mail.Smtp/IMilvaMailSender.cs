using System.Net.Mail;
using System.Net.Mime;

namespace Milvasoft.Notification.Mail.Smtp;

/// <summary>
/// Provides send mail.
/// </summary>
public interface IMilvaMailSender
{
    #region Async

    /// <summary>
    /// Provides send mail.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isBodyHtml"></param>
    Task MilvaSendMailAsync(string to,
                            string subject,
                            string body,
                            bool isBodyHtml = false);

    #region Send With Attachment

    /// <summary>
    /// Provides send mail with attachment.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="filePath"></param>
    /// <param name="contentType"></param>
    /// <param name="isBodyHtml"></param>
    Task MilvaSendMailAsync(string to,
                            string subject,
                            string body,
                            string filePath,
                            ContentType contentType,
                            bool isBodyHtml = false);

    /// <summary>
    /// Provides send mail with attachment.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="attachments"></param>
    /// <param name="isBodyHtml"></param>
    Task MilvaSendMailAsync(string to,
                            string subject,
                            string body,
                            List<Attachment> attachments,
                            bool isBodyHtml = false);

    /// <summary>
    /// Provides send mail with attachment.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="base64String"> Data uri formatted base64 string.</param>
    /// <param name="attachmentName"></param>
    /// <param name="isBodyHtml"></param>
    Task MilvaSendMailAsync(string to,
                            string subject,
                            string body,
                            string base64String,
                            string attachmentName,
                            bool isBodyHtml = false);

    #endregion

    #endregion

    #region Sync

    /// <summary>
    /// Provides send mail.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isBodyHtml"></param>
    void MilvaSendMail(string to,
                       string subject,
                       string body,
                       bool isBodyHtml = false);

    /// <summary>
    /// Provides send mail with attachment.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="filePath"></param>
    /// <param name="isBodyHtml"></param>
    void MilvaSendMail(string to,
                       string subject,
                       string body,
                       string filePath,
                       bool isBodyHtml = false);

    #endregion
}
