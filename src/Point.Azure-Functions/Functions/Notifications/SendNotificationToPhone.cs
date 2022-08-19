namespace Point.Azure_Functions.Functions.Notifications;

public static class SendNotificationToPhone
{
    [FunctionName("SendNotificationToPhone")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SendToPhone")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("---- Created Notification message: SMS Notification");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        SmsToSend message = JsonConvert.DeserializeObject<SmsToSend>(requestBody);

        TwilioClient.Init(Environment.GetEnvironmentVariable("AccountSID"),
            Environment.GetEnvironmentVariable("AuthToken"));

        MessageResource.Create(
            body: message.Content,
            from: new Twilio.Types.PhoneNumber(Environment.GetEnvironmentVariable("PhoneFrom")),
            to: new Twilio.Types.PhoneNumber(message.Recipient)
        );

        log.LogInformation("---- Sent Notification message: SMS Notification");
        return new OkObjectResult("");
    }
}

