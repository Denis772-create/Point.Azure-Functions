namespace Point.Azure_Functions.Helpers;

public class ServiceBusAdmin
{
    private readonly ServiceBusAdministrationClient _subscriptionClient;

    public ServiceBusAdmin(string connectionsString)
    {
        _subscriptionClient = new ServiceBusAdministrationClient(connectionsString);
    }

    public async Task CreateRulesForTopic(string subjectName)
    {
        await _subscriptionClient.CreateRuleAsync(
            Environment.GetEnvironmentVariable("TopicName"),
            Environment.GetEnvironmentVariable("SubscriptionName"),
            new CreateRuleOptions
            {
                Filter = new CorrelationRuleFilter { Subject = subjectName },
                Name = subjectName
            });
    }
}