namespace Point.Azure_Functions.Functions.Notifications;

public class SendNotificationToPhone
{
    private readonly ILogger<SendNotificationToPhone> _log;
    private readonly ServiceBusAdmin _busAdmin;

    public record SmsToSend(string Recipient, string Content);

    public SendNotificationToPhone(ILogger<SendNotificationToPhone> log)
    {
        _log = log;
        _busAdmin = new ServiceBusAdmin(
            Environment.GetEnvironmentVariable("AzureWebJobsServiceBus"));
    }

    [FunctionName("SendNotificationToPhone")]
    public async Task Run([ServiceBusTrigger("point_event_bus", "Notification")] SmsToSend smsToSend)
    {
        try
        {
            await _busAdmin.CreateRulesForTopic(nameof(SmsToSend));
        }
        catch
        {
            Console.WriteLine("Topic with such rules already exist!");
            if (smsToSend.Recipient == null) return;
        }

        _log.LogInformation("---- Created Notification message: SMS Notification");

        TwilioClient.Init(Environment.GetEnvironmentVariable("AccountSID"),
            Environment.GetEnvironmentVariable("AuthToken"));

        await MessageResource.CreateAsync(
            body: smsToSend.Content,
            from: new Twilio.Types.PhoneNumber(Environment.GetEnvironmentVariable("PhoneFrom")),
            to: new Twilio.Types.PhoneNumber(smsToSend.Recipient)
        );

        _log.LogInformation("---- Sent Notification message: SMS Notification");
    }
}