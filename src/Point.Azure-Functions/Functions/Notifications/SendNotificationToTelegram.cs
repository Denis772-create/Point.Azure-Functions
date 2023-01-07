namespace Point.Azure_Functions.Functions.Notifications;

public class SendNotificationToTelegram
{
    private readonly ILogger<SendNotificationToTelegram> _log;
    private readonly ServiceBusAdmin _busAdmin;

    public record TelegramToSend(string ChannelId, string Content);

    public SendNotificationToTelegram(ILogger<SendNotificationToTelegram> log)
    {
        _log = log;
        _busAdmin = new ServiceBusAdmin(
            Environment.GetEnvironmentVariable("AzureWebJobsServiceBus"));
    }
    [FunctionName("SendNotificationToTelegram")]
    public async Task Run([ServiceBusTrigger("point_event_bus", "Notification")]
        TelegramToSend telegramToSend)
    {
        try
        {
            await _busAdmin.CreateRulesForTopic(nameof(TelegramToSend));
        }
        catch
        {
            Console.WriteLine("Topic with such rules already exist!");
            if (telegramToSend.ChannelId == null) return;
        }

        var bot = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramToken"));

        var t = Environment.GetEnvironmentVariable("ChannelId");
        await bot.SendTextMessageAsync(telegramToSend.ChannelId,
            telegramToSend.Content,
            parseMode: ParseMode.Markdown);

        _log.LogInformation("---- Sent Notification message: Telegram Notification");
    }
}