namespace Point.Azure_Functions.Functions.Notifications;

public static class SendNotificationToPhone
{
    [FunctionName("SendNotificationToPhone")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SendToPhone")] HttpRequest req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        return new OkObjectResult("");
    }
}

