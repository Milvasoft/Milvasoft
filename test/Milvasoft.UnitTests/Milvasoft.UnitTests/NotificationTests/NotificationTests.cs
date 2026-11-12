using FluentAssertions;
using Milvasoft.Core.Exceptions;
using Milvasoft.Notification.Mail.Smtp;
using Milvasoft.Notification.Push.Expo.PushNotification;
using System.Net;

namespace Milvasoft.UnitTests.NotificationTests;

[Trait("Notification Unit Tests", "Milvasoft.Notification projects unit tests.")]
public class NotificationTests
{
    private static MilvaMailSender CreateSender(int port = 465, bool enableSsl = true, string host = "smtp.example.com")
        => new("from@example.com", new NetworkCredential("user", "pass"), port, host, enableSsl);

    /// <summary>
    /// The previous implementation built a NonBacktracking regex with a lookahead (NotSupportedException) and base64-decoded the
    /// whole data URI (FormatException), so this overload could never send. Here parsing succeeds and sending stops at the
    /// transport validation (port 465), which runs after the attachment was built.
    /// </summary>
    [Fact]
    public Task MilvaSendMailAsync_WithDataUriAttachment_ShouldParseAttachment()
    {
        var sender = CreateSender();
        var dataUri = "data:application/pdf;base64," + Convert.ToBase64String([0x25, 0x50, 0x44, 0x46]);

        var act = () => sender.MilvaSendMailAsync("to@example.com", "subject", "body", dataUri, "file");

        return act.Should().ThrowAsync<MilvaDeveloperException>().WithMessage("*465*");
    }

    [Fact]
    public Task MilvaSendMailAsync_WithMalformedDataUri_ShouldThrowArgumentException()
    {
        var sender = CreateSender(port: 587);

        var act = () => sender.MilvaSendMailAsync("to@example.com", "subject", "body", "not-a-data-uri", "file");

        return act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// The synchronous methods used fire-and-forget SendAsync, so errors were never reported to the caller.
    /// </summary>
    [Fact]
    public void MilvaSendMail_Sync_ShouldSurfaceErrorsToCaller()
    {
        var sender = CreateSender();

        var act = () => sender.MilvaSendMail("to@example.com", "subject", "body");

        act.Should().Throw<MilvaDeveloperException>();
    }

    [Fact]
    public Task MilvaSendMailAsync_WithCredentialsWithoutSsl_ShouldRefuse()
    {
        var sender = CreateSender(port: 25, enableSsl: false);

        var act = () => sender.MilvaSendMailAsync("to@example.com", "subject", "body");

        return act.Should().ThrowAsync<MilvaDeveloperException>().WithMessage("*unencrypted*");
    }

    [Fact]
    public Task MilvaSendMailAsync_WithCancelledToken_ShouldNotSend()
    {
        var sender = CreateSender(port: 587, host: "127.0.0.1");

        var act = () => sender.MilvaSendMailAsync("to@example.com", "subject", "body", cancellationToken: new CancellationToken(true));

        return act.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Disposing an ExpoClient instance disposed the static HttpClient shared by every instance.
    /// </summary>
    [Fact]
    public Task ExpoClient_Dispose_ShouldNotDisposeSharedHttpClient()
    {
        new ExpoClient().Dispose();

        var act = () => ExpoClient.PushGetReceiptsAsync(new PushReceiptRequest { PushTicketIds = ["id"] }, new CancellationToken(true));

        // A disposed client throws ObjectDisposedException; a live one honors the cancelled token.
        return act.Should().ThrowAsync<OperationCanceledException>();
    }
}
