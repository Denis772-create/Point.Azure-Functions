using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Point.Azure_Functions.Functions.Notifications;

public static class SendNotificationToTelegram
{
    [FunctionName("SendNotificationToTelegram")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SendToTelegram")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("---- Created Notification message: Telegram Notification");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        TelegramToSend message = JsonConvert.DeserializeObject<TelegramToSend>(requestBody);

        var bot = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramToken"));

        var t = Environment.GetEnvironmentVariable("ChannelId");
        await bot.SendTextMessageAsync(message.ChannelId,
            message.Content, 
            parseMode: ParseMode.Markdown);

        log.LogInformation("---- Sent Notification message: Telegram Notification");
        return new OkObjectResult("Message sent successfully!");
    }
}

