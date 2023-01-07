namespace Point.Azure_Functions.Functions.Notifications;

public class SendNotificationToEmail
{
    private readonly ILogger<SendNotificationToEmail> _log;
    private readonly ServiceBusAdmin _busAdmin;

    public record EmailToSend(string To, string Subject, string PlainBody, string HtmlBody);

    public SendNotificationToEmail(ILogger<SendNotificationToEmail> log)
    {
        _log = log;
        _busAdmin = new ServiceBusAdmin(Environment.GetEnvironmentVariable("AzureWebJobsServiceBus"));
    }

    [FunctionName("SendNotificationToEmail")]
    public async Task Run([ServiceBusTrigger("point_event_bus", "Notification")] EmailToSend emailToSend)
    {
        try
        {
            await _busAdmin.CreateRulesForTopic(nameof(EmailToSend));
        }
        catch
        {
            Console.WriteLine("Topic with such rules already exist!");
            if (emailToSend.To == null) return;
        }

        try
        {
            List<string> toEmailsList = GetToEmailAddressList(emailToSend.To);

            _log.LogInformation("Calling the SendEmail method...");
            await SendEmail(
                Environment.GetEnvironmentVariable("Host"),
                int.Parse(Environment.GetEnvironmentVariable("Port")),
                bool.Parse(Environment.GetEnvironmentVariable("HostUsesLocalCertificate")),
                Environment.GetEnvironmentVariable("User"),
                Environment.GetEnvironmentVariable("Password"),
                Environment.GetEnvironmentVariable("FromName"),
                Environment.GetEnvironmentVariable("FromEmail"),
                toEmailsList,
                emailToSend.Subject,
                emailToSend.PlainBody,
                emailToSend.HtmlBody,
                null,
                null
            );

            _log.LogInformation($"Email to: {string.Join(";", toEmailsList.ToArray())} sent successfully!");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, ex.Message);
            throw;
        }
    }

    public static List<string> GetToEmailAddressList(string toEmails)
    {
        if (string.IsNullOrWhiteSpace(toEmails))
            return new() { Environment.GetEnvironmentVariable("FromEmail") };

        return new(toEmails.Split(";"));
    }

    public static async Task SendEmail(string host, int port, bool hostUsesLocalCertificate,
           string user, string password, string fromName, string fromEmail,
           List<string> ToEmails, string subject, string bodyPlain, string bodyHtml,
           string linkedResourcePath, string attachmentPath)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.Subject = subject;

        foreach (var toEmail in ToEmails)
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