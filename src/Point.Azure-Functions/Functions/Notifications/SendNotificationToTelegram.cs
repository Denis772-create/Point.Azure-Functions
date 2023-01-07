namespace Point.Azure_Functions.Functions.Notifications;

public class SendNotificationToTelegram
{
    private readonly ILogger<SendNotificationToTelegram> _log;
    private readonly TelegramOptions _options;

    public record TelegramToSend
    {
        public string ChannelId { get; init; }
        public string Content { get; init; }
    }

    public SendNotificationToTelegram(ILogger<SendNotificationToTelegram> log, TelegramOptions options)
    {
        _log = log;
        _options = options;
    }

    [FunctionName("SendNotificationToTelegram")]
    public async Task Run([ServiceBusTrigger(Constants.TopicName, Constants.TelegramSubscriptionName)] TelegramToSend telegramToSend)
    {
        if (telegramToSend.ChannelId == null) return;

        var bot = new TelegramBotClient(_options.TelegramToken);
        await bot.SendTextMessageAsync(telegramToSend.ChannelId, telegramToSend.Content, ParseMode.Markdown);

        _log.LogInformation("Sent notification message to Telegram");
    }
}