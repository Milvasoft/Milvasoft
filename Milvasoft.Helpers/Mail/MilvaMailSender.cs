﻿using Milvasoft.Helpers.Enums;
using Milvasoft.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.Mail
{
    /// <summary>
    /// Provides send mail.
    /// </summary>
    public class MilvaMailSender : IMilvaMailSender
    {
        /// <summary>
        /// Gets or sets mail sender.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets mail sender credentials.
        /// </summary>
        public NetworkCredential NetworkCredential { get; set; }

        /// <summary>
        /// Gets or sets Port of mail sender.
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets Host of mail sender.
        /// </summary>
        public string SmtpHost { get; set; }

        /// <summary>
        /// Initializes mail sending operation default values.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="networkCredential"></param>
        /// <param name="smtpPort"></param>
        /// <param name="smtpHost"></param>
        public MilvaMailSender(string from, NetworkCredential networkCredential, int smtpPort, string smtpHost)
        {
            From = from;
            NetworkCredential = networkCredential;
            SmtpPort = smtpPort;
            SmtpHost = smtpHost;
        }

        #region Async

        /// <summary>
        /// Provides send mail.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isBodyHtml"></param>
        public async Task MilvaSendMailAsync(string to,
                                             string subject,
                                             string body,
                                             bool isBodyHtml = false) => await SendMailAsync(to, subject, body, isBodyHtml).ConfigureAwait(false);

        /// <summary>
        /// Provides send mail.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isBodyHtml"></param>
        public async Task MilvaSendMailAsync(string to,
                                             MailSubject subject,
                                             string body,
                                             bool isBodyHtml = false) => await SendMailAsync(to, CommonHelper.GetEnumDesciption(subject), body, isBodyHtml).ConfigureAwait(false);

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
        public async Task MilvaSendMailAsync(string to,
                                             string subject,
                                             string body,
                                             string filePath,
                                             ContentType contentType,
                                             bool isBodyHtml = false) => await SendMailWithFileAsync(to, subject, body, isBodyHtml, filePath, contentType).ConfigureAwait(false);

        /// <summary>
        /// Provides send mail with attachment.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachments"></param>
        /// <param name="isBodyHtml"></param>
        public async Task MilvaSendMailAsync(string to,
                                             string subject,
                                             string body,
                                             List<Attachment> attachments,
                                             bool isBodyHtml = false)
        {
            using var mailMessage = new MailMessage(From, to, subject, body)
            {
                IsBodyHtml = isBodyHtml
            };

            if (!attachments.IsNullOrEmpty())
                foreach (var attachment in attachments)
                {
                    mailMessage.Attachments.Add(attachment);
                }

            using var smtpClient = new SmtpClient(SmtpHost, SmtpPort)
            {
                Credentials = NetworkCredential
            };

            await smtpClient.SendMailAsync(mailMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Provides send mail with attachment.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="base64String"> Data uri formatted base64 string.</param>
        /// <param name="attachmentName"></param>
        /// <param name="isBodyHtml"></param>
        public async Task MilvaSendMailAsync(string to,
                                             string subject,
                                             string body,
                                             string base64String,
                                             string attachmentName,
                                             bool isBodyHtml = false)
        {
            var base64 = base64String.Split(";base64,")?[1];

            var regex = @"[^:]\w+\/[\w-+\d.]+(?=;|,)";

            var ct = new Regex(regex).Match(base64).Captures?.FirstOrDefault()?.Value;

            var splittedContentType = ct.Split('/');

            var fileExtension = splittedContentType?[1];

            var array = Convert.FromBase64String(base64String);

            using var ms = new MemoryStream(array)
            {
                Position = 0
            };

            var contentType = new ContentType(ct);

            using var attachment = new Attachment(ms, contentType);

            attachment.ContentDisposition.FileName = $"{attachmentName}.{fileExtension}";

            using var mailMessage = new MailMessage(From, to, subject, body)
            {
                IsBodyHtml = isBodyHtml
            };

            // Add the file attachment to this email message.
            mailMessage.Attachments.Add(attachment);

            using var smtpClient = new SmtpClient(SmtpHost, SmtpPort)
            {
                Credentials = NetworkCredential
            };

            await smtpClient.SendMailAsync(mailMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Provides send mail with attachment.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="filePath"></param>
        /// <param name="contentType"></param>
        /// <param name="isBodyHtml"></param>
        public async Task MilvaSendMailAsync(string to,
                                             MailSubject subject,
                                             string body,
                                             string filePath,
                                             ContentType contentType,
                                             bool isBodyHtml = false) => await SendMailWithFileAsync(to,
                                                                                                     CommonHelper.GetEnumDesciption(subject),
                                                                                                     body,
                                                                                                     isBodyHtml,
                                                                                                     filePath,
                                                                                                     contentType).ConfigureAwait(false);

        #endregion

        #region Private Methods

        private async Task SendMailAsync(string to, string subject, string body, bool isBodyHtml)
        {
            using var mailMessage = new MailMessage(From, to, subject, body)
            {
                IsBodyHtml = isBodyHtml
            };

            using var smtpClient = new SmtpClient(SmtpHost, SmtpPort);
            smtpClient.Credentials = NetworkCredential;

            await smtpClient.SendMailAsync(mailMessage).ConfigureAwait(false);
        }

        private async Task SendMailWithFileAsync(string to, string subject, string body, bool isBodyHtml, string filePath, ContentType contentType)
        {
            using var mailMessage = new MailMessage(From, to, subject, body)
            {
                IsBodyHtml = isBodyHtml
            };

            // Create  the file attachment for this email message.
            using var attachment = new Attachment(filePath, contentType);

            // Add time stamp information for the file.
            var disposition = attachment.ContentDisposition;

            disposition.CreationDate = File.GetCreationTime(filePath);

            disposition.ModificationDate = File.GetLastWriteTime(filePath);

            disposition.ReadDate = File.GetLastAccessTime(filePath);

            // Add the file attachment to this email message.
            mailMessage.Attachments.Add(attachment);

            using var smtpClient = new SmtpClient(SmtpHost, SmtpPort)
            {
                Credentials = NetworkCredential
            };

            await smtpClient.SendMailAsync(mailMessage).ConfigureAwait(false);
        }

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
        public void MilvaSendMail(string to,
                                  string subject,
                                  string body,
                                  bool isBodyHtml = false) => SendMail(to, subject, body, isBodyHtml);

        /// <summary>
        /// Provides send mail with attachment.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="filePath"></param>
        /// <param name="isBodyHtml"></param>
        public void MilvaSendMail(string to,
                                  string subject,
                                  string body,
                                  string filePath,
                                  bool isBodyHtml = false) => SendMailWithFile(to, subject, body, isBodyHtml, filePath);

        /// <summary>
        /// Provides send mail.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isBodyHtml"></param>
        public void MilvaSendMail(string to,
                                  MailSubject subject,
                                  string body,
                                  bool isBodyHtml = false) => SendMail(to, CommonHelper.GetEnumDesciption(subject), body, isBodyHtml);


        #region Private Methods

        private void SendMail(string to, string subject, string body, bool isBodyHtml)
        {
            var mailMessage = new MailMessage(From, to, subject, body)
            {
                IsBodyHtml = isBodyHtml
            };

            var smtpClient = new SmtpClient(SmtpHost, SmtpPort);
            smtpClient.Credentials = NetworkCredential;

            smtpClient.SendAsync(mailMessage, mailMessage);
        }

        private void SendMailWithFile(string to, string subject, string body, bool isBodyHtml, string filePath)
        {
            var mailMessage = new MailMessage(From, to, subject, body)
            {
                IsBodyHtml = isBodyHtml
            };

            // Create  the file attachment for this email message.
            var data = new Attachment(filePath, MediaTypeNames.Application.Octet);

            // Add time stamp information for the file.
            var disposition = data.ContentDisposition;

            disposition.CreationDate = File.GetCreationTime(filePath);

            disposition.ModificationDate = File.GetLastWriteTime(filePath);

            disposition.ReadDate = File.GetLastAccessTime(filePath);

            // Add the file attachment to this email message.
            mailMessage.Attachments.Add(data);

            var smtpClient = new SmtpClient(SmtpHost, SmtpPort)
            {
                Credentials = NetworkCredential
            };

            smtpClient.SendAsync(mailMessage, mailMessage);
        }


        #endregion

        #endregion
    }
}
