namespace Point.Azure_Functions.Functions.Notifications;

public static class SendNotificationToTelegram
{
    [FunctionName("SendNotificationToTelegram")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SendToTelegram")] HttpRequest req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        return new OkObjectResult("");
    }
}

