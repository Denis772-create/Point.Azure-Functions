namespace Point.Azure_Functions.Functions.Notifications;

public class NotificationToTelegram
{
    private readonly ILogger<NotificationToTelegram> _log;
    private readonly TelegramOptions _options;

    public record TelegramToSend
    {
        public string ChannelId { get; init; }
        public string Content { get; init; }
    }

    public NotificationToTelegram(ILogger<NotificationToTelegram> log, TelegramOptions options)
    {
        _log = log;
        _options = options;
    }

    //TODO: remove comment
    //[FunctionName("SendNotificationToTelegram")]
    public async Task Run([ServiceBusTrigger(Constants.TopicName, Constants.TelegramSubscriptionName)] TelegramToSend telegramToSend)
    {
        if (telegramToSend.ChannelId == null) return;

        await new TelegramBotClient(_options.TelegramToken)
            .SendTextMessageAsync(telegramToSend.ChannelId, telegramToSend.Content, ParseMode.Markdown);

        _log.LogInformation("Sent notification message to Telegram");
    }
}