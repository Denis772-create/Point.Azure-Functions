namespace Point.Azure_Functions.Functions.Notifications;

public class NotificationToPhone
{
    private readonly ILogger<NotificationToPhone> _log;
    private readonly PhoneOptions _options;

    public record SmsToSend
    {
        public string Recipient { get; init; }
        public string Content { get; init; }
    }

    public NotificationToPhone(ILogger<NotificationToPhone> log, PhoneOptions options)
    {
        _log = log;
        _options = options;
    }

    //TODO: remove comment
    //[FunctionName("SendNotificationToPhone")]
    public async Task Run([ServiceBusTrigger(Constants.TopicName, Constants.SmsSubscriptionName)] SmsToSend smsToSend)
    {
        if (smsToSend.Recipient == null) return;

        TwilioClient.Init(_options.AccountSid, _options.AuthToken);

        await MessageResource.CreateAsync(
            body: smsToSend.Content,
            from: new Twilio.Types.PhoneNumber(_options.PhoneFrom),
            to: new Twilio.Types.PhoneNumber(smsToSend.Recipient)
        );

        _log.LogInformation($"Sent SMS Notification message to {smsToSend.Recipient}");
    }
}