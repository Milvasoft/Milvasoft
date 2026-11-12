using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Helpers;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace Milvasoft.Notification.Mail.Smtp;

/// <summary>
/// Provides send mail.
/// </summary>
/// <remarks>
/// <para>Initializes mail sending operation default values.</para>
/// <para>
/// Uses <see cref="SmtpClient"/>, which supports only explicit TLS (STARTTLS, usually port 587). Implicit TLS (SMTPS, port 465)
/// is not supported and is rejected instead of hanging until the timeout. Credentials are not sent over an unencrypted connection
/// unless <see cref="AllowUnencryptedCredentials"/> is set.
/// </para>
/// <para>
/// <paramref name="from"/> and the <c>to</c> parameter of the send methods accept a comma separated address list
/// (see <see cref="MailAddressCollection.Add(string)"/>).
/// </para>
/// </remarks>
/// <param name="from"></param>
/// <param name="networkCredential"></param>
/// <param name="smtpPort"></param>
/// <param name="smtpHost"></param>
/// <param name="enableSsl"></param>
public class MilvaMailSender(string from, NetworkCredential networkCredential, int smtpPort, string smtpHost, bool enableSsl) : IMilvaMailSender
{
    private const int _implicitTlsPort = 465;
    private const string _base64Flag = ";base64,";
    private const string _dataUriPrefix = "data:";

    /// <summary>
    /// Gets or sets mail sender.
    /// </summary>
    public string From { get; set; } = from;

    /// <summary>
    /// Gets or sets mail sender credentials.
    /// </summary>
    public NetworkCredential NetworkCredential { get; set; } = networkCredential;

    /// <summary>
    /// Gets or sets Port of mail sender.
    /// </summary>
    public int SmtpPort { get; set; } = smtpPort;

    /// <summary>
    /// Gets or sets Host of mail sender.
    /// </summary>
    public string SmtpHost { get; set; } = smtpHost;

    /// <summary>
    /// Gets or sets enable ssl (STARTTLS) of mail sender smtp client.
    /// </summary>
    public bool EnableSsl { get; set; } = enableSsl;

    /// <summary>
    /// Gets or sets whether credentials may be sent when <see cref="EnableSsl"/> is false. Default is false
    /// (credentials would travel in clear text). Connections to a loopback host are always allowed.
    /// </summary>
    public bool AllowUnencryptedCredentials { get; set; }

    /// <summary>
    /// Gets or sets the SMTP timeout in milliseconds. Default is 100 seconds.
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 100_000;

    #region Async

    /// <summary>
    /// Provides send mail.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isBodyHtml"></param>
    /// <param name="cancellationToken"></param>
    public async Task MilvaSendMailAsync(string to, string subject, string body, bool isBodyHtml = false, CancellationToken cancellationToken = default)
    {
        using var mailMessage = CreateMessage(to, subject, body, isBodyHtml);

        await SendAsync(mailMessage, cancellationToken);
    }

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
    /// <param name="cancellationToken"></param>
    public async Task MilvaSendMailAsync(string to,
                                         string subject,
                                         string body,
                                         string filePath,
                                         ContentType contentType,
                                         bool isBodyHtml = false,
                                         CancellationToken cancellationToken = default)
    {
        using var mailMessage = CreateMessage(to, subject, body, isBodyHtml);

        mailMessage.Attachments.Add(CreateFileAttachment(filePath, contentType));

        await SendAsync(mailMessage, cancellationToken);
    }

    /// <summary>
    /// Provides send mail with attachment.
    /// </summary>
    /// <remarks>The attachments are disposed with the message after sending.</remarks>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="attachments"></param>
    /// <param name="isBodyHtml"></param>
    /// <param name="cancellationToken"></param>
    public async Task MilvaSendMailAsync(string to,
                                         string subject,
                                         string body,
                                         List<Attachment> attachments,
                                         bool isBodyHtml = false,
                                         CancellationToken cancellationToken = default)
    {
        using var mailMessage = CreateMessage(to, subject, body, isBodyHtml);

        if (!attachments.IsNullOrEmpty())
            foreach (var attachment in attachments)
                mailMessage.Attachments.Add(attachment);

        await SendAsync(mailMessage, cancellationToken);
    }

    /// <summary>
    /// Provides send mail with attachment.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="base64String"> Data uri formatted base64 string (e.g. <c>data:application/pdf;base64,JVBERi0...</c>).</param>
    /// <param name="attachmentName"></param>
    /// <param name="isBodyHtml"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentException">If <paramref name="base64String"/> is not a base64 data URI.</exception>
    public async Task MilvaSendMailAsync(string to,
                                         string subject,
                                         string body,
                                         string base64String,
                                         string attachmentName,
                                         bool isBodyHtml = false,
                                         CancellationToken cancellationToken = default)
    {
        var (content, contentType) = ParseDataUri(base64String);

        var fileExtension = contentType.MediaType.Split('/') is { Length: 2 } parts ? parts[1] : "bin";

        using var mailMessage = CreateMessage(to, subject, body, isBodyHtml);

        var attachment = new Attachment(new MemoryStream(content, writable: false), contentType);

        attachment.ContentDisposition.FileName = $"{attachmentName}.{fileExtension}";

        mailMessage.Attachments.Add(attachment);

        await SendAsync(mailMessage, cancellationToken);
    }

    #endregion

    #endregion

    #region Sync

    /// <summary>
    /// Provides send mail. Blocks until the message is sent.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="isBodyHtml"></param>
    public void MilvaSendMail(string to, string subject, string body, bool isBodyHtml = false)
    {
        using var mailMessage = CreateMessage(to, subject, body, isBodyHtml);

        Send(mailMessage);
    }

    /// <summary>
    /// Provides send mail with attachment. Blocks until the message is sent.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="filePath"></param>
    /// <param name="isBodyHtml"></param>
    public void MilvaSendMail(string to, string subject, string body, string filePath, bool isBodyHtml = false)
    {
        using var mailMessage = CreateMessage(to, subject, body, isBodyHtml);

        mailMessage.Attachments.Add(CreateFileAttachment(filePath, new ContentType(MediaTypeNames.Application.Octet)));

        Send(mailMessage);
    }

    #endregion

    #region Private Methods

    private MailMessage CreateMessage(string to, string subject, string body, bool isBodyHtml) => new(From, to, subject, body)
    {
        IsBodyHtml = isBodyHtml
    };

    private static Attachment CreateFileAttachment(string filePath, ContentType contentType)
    {
        var attachment = new Attachment(filePath, contentType);

        var disposition = attachment.ContentDisposition;

        disposition.CreationDate = File.GetCreationTime(filePath);
        disposition.ModificationDate = File.GetLastWriteTime(filePath);
        disposition.ReadDate = File.GetLastAccessTime(filePath);

        return attachment;
    }

    private async Task SendAsync(MailMessage mailMessage, CancellationToken cancellationToken)
    {
        using var smtpClient = CreateSmtpClient();

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }

    private void Send(MailMessage mailMessage)
    {
        using var smtpClient = CreateSmtpClient();

        smtpClient.Send(mailMessage);
    }

    /// <summary>
    /// Creates a configured <see cref="SmtpClient"/> after validating the transport settings.
    /// </summary>
    internal SmtpClient CreateSmtpClient()
    {
        if (SmtpPort == _implicitTlsPort)
            throw new MilvaDeveloperException("SmtpClient does not support implicit TLS (port 465). Use port 587 with EnableSsl (STARTTLS).");

        if (NetworkCredential != null && !EnableSsl && !AllowUnencryptedCredentials && !IsLoopback(SmtpHost))
            throw new MilvaDeveloperException("Refusing to send SMTP credentials over an unencrypted connection. Enable SSL or set AllowUnencryptedCredentials.");

        return new SmtpClient(SmtpHost, SmtpPort)
        {
            Credentials = NetworkCredential,
            EnableSsl = EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = TimeoutMilliseconds,
        };
    }

    private static bool IsLoopback(string host)
        => string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) || IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address);

    /// <summary>
    /// Parses a base64 data URI (<c>data:[media type][;params];base64,[data]</c>).
    /// </summary>
    internal static (byte[] Content, ContentType ContentType) ParseDataUri(string dataUri)
    {
        var flagIndex = dataUri?.IndexOf(_base64Flag, StringComparison.Ordinal) ?? -1;

        if (flagIndex < 0)
            throw new ArgumentException("Value is not a base64 data URI.", nameof(dataUri));

        var header = dataUri[..flagIndex];

        if (header.StartsWith(_dataUriPrefix, StringComparison.OrdinalIgnoreCase))
            header = header[_dataUriPrefix.Length..];

        var parameterIndex = header.IndexOf(';');
        var mediaType = (parameterIndex >= 0 ? header[..parameterIndex] : header).Trim();

        if (string.IsNullOrEmpty(mediaType))
            mediaType = MediaTypeNames.Application.Octet;

        byte[] content;

        try
        {
            content = Convert.FromBase64String(dataUri[(flagIndex + _base64Flag.Length)..]);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Value is not a base64 data URI.", nameof(dataUri), ex);
        }

        return (content, new ContentType(mediaType));
    }

    #endregion
}
