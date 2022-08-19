namespace Point.Azure_Functions.Functions.Notifications;

public static class SendNotificationToEmail
{
    [FunctionName("SendNotificationToEmail")]
    public static async Task<IActionResult> SendToEmail(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SendToEmail")] HttpRequest req,
        ILogger log)
    {
        try
        {
            log.LogInformation("Parsing the request body to retrieve the email to send...");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EmailToSend emailToSend = JsonConvert.DeserializeObject<EmailToSend>(requestBody);

            log.LogInformation("Parsing the To Emails...");
            List<string> toEmailsList = GetToEmailAddressList(emailToSend.To);

            log.LogInformation("Calling the SendEmail method...");
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

            log.LogInformation($"Email to: {string.Join(";", toEmailsList.ToArray())} sent successfully!");
            return new OkObjectResult("Email sent successfully!");
        }
        catch (Exception ex)
        {
            log.LogError(ex, ex.Message);
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
           string user, string password,string fromName, string fromEmail, 
           List<string> ToEmails, string subject, string bodyPlain, string bodyHtml,
           string linkedResourcePath, string attachmentPath)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.Subject = subject;

        foreach (string toEmail in ToEmails)
            message.To.Add(new MailboxAddress(toEmail, toEmail));

        var builder = new BodyBuilder();

        if (!string.IsNullOrWhiteSpace(bodyPlain))
            builder.TextBody = bodyPlain;

        if (!string.IsNullOrWhiteSpace(bodyHtml))
        {
            if (!string.IsNullOrWhiteSpace(linkedResourcePath))
            {
                MimeEntity image = builder.LinkedResources.Add(linkedResourcePath);
                image.ContentId = MimeUtils.GenerateMessageId();
                builder.HtmlBody = string.Format(bodyHtml, image.ContentId);
            }
            else
                builder.HtmlBody = bodyHtml;
        }

        if (!string.IsNullOrWhiteSpace(attachmentPath))
            builder.Attachments.Add(attachmentPath);

        message.Body = builder.ToMessageBody();

        using var smtpClient = new MailKit.Net.Smtp.SmtpClient();

        if (hostUsesLocalCertificate)
            smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;

        await smtpClient.ConnectAsync(host, port);
        await smtpClient.AuthenticateAsync(user, password);
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}

