using System.Runtime.Serialization;

namespace Point.Azure_Functions.Functions.Notifications;

public class SendNotificationToEmail
{
    private readonly ILogger<SendNotificationToEmail> _log;
    private readonly EmailOptions _options;

    public record EmailToSend
    {
        public string To { get; init; }
        public string MessageSubject { get; init; }
        public string PlainBody { get; init; }
        public string HtmlBody { get; init; }
    }

    public SendNotificationToEmail(ILogger<SendNotificationToEmail> log, EmailOptions options)
    {
        _log = log;
        _options = options;
    }

    [FunctionName("SendNotificationToEmail")]
    public async Task Run([ServiceBusTrigger(Constants.TopicName, Constants.EmailSubscriptionName)] EmailToSend emailToSend)
    {
        if (emailToSend.To == null) return;

        try
        {
            var toEmailsList = GetToEmailAddressList(emailToSend.To);

            await SendEmail(_options.Host, _options.Port, _options.HostUsesLocalCertificate, _options.User, _options.Password, _options.FromName, _options.FromEmail,
                toEmailsList, emailToSend.MessageSubject, emailToSend.PlainBody, emailToSend.HtmlBody,
                null, null);

            _log.LogInformation($"Email to: {string.Join(";", toEmailsList.ToArray())} sent successfully!");
        }
        catch (Exception ex)
        {
            // TODO: log to app insights
            _log.LogError(ex, ex.Message);
            throw new Exception();
        }
    }

    public List<string> GetToEmailAddressList(string toEmails)
    {
        return string.IsNullOrWhiteSpace(toEmails)
            ? new() { _options.FromEmail }
            : new(toEmails.Split(";"));
    }

    public static async Task SendEmail(string host, int port, bool hostUsesLocalCertificate,
           string user, string password, string fromName, string fromEmail,
           List<string> toEmails, string subject, string bodyPlain, string bodyHtml,
           string linkedResourcePath, string attachmentPath)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.Subject = subject;

        foreach (var toEmail in toEmails)
            message.To.Add(new MailboxAddress(toEmail, toEmail));

        var builder = new BodyBuilder();

        if (!string.IsNullOrWhiteSpace(bodyPlain))
            builder.TextBody = bodyPlain;

        if (!string.IsNullOrWhiteSpace(bodyHtml))
        {
            if (!string.IsNullOrWhiteSpace(linkedResourcePath))
            {
                MimeEntity image = await builder.LinkedResources.AddAsync(linkedResourcePath);
                image.ContentId = MimeUtils.GenerateMessageId();
                builder.HtmlBody = string.Format(bodyHtml, image.ContentId);
            }
            else
                builder.HtmlBody = bodyHtml;
        }

        if (!string.IsNullOrWhiteSpace(attachmentPath))
            await builder.Attachments.AddAsync(attachmentPath);

        message.Body = builder.ToMessageBody();

        using var smtpClient = new MailKit.Net.Smtp.SmtpClient();

        if (hostUsesLocalCertificate)
            smtpClient.ServerCertificateValidationCallback = (_, _, _, _) => true;

        await smtpClient.ConnectAsync(host, port);
        await smtpClient.AuthenticateAsync(user, password);
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}