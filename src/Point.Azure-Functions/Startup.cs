using static Point.Azure_Functions.Functions.Notifications.NotificationToEmail;
using static Point.Azure_Functions.Functions.Notifications.NotificationToPhone;
using static Point.Azure_Functions.Functions.Notifications.NotificationToTelegram;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Point.Azure_Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;

        var baseConfiguration = configuration.GetSection("Values").Get<BaseOptions>();
        builder.Services.AddSingleton(baseConfiguration);

        var emailConfiguration = configuration.GetSection("Email").Get<EmailOptions>();
        builder.Services.AddSingleton(emailConfiguration);

        //TODO: remove comment
        //var telegramConfiguration = configuration.GetSection("Telegram").Get<TelegramOptions>();
        //builder.Services.AddSingleton(telegramConfiguration);

        //var smsConfiguration = configuration.GetSection("Sms").Get<PhoneOptions>();
        //builder.Services.AddSingleton(smsConfiguration);

        ConfigureAzureServiceBus(baseConfiguration.AzureWebJobsServiceBus).GetAwaiter();
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        var context = builder.GetContext();

        builder.ConfigurationBuilder
            .SetBasePath(context.ApplicationRootPath)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    private static async Task ConfigureAzureServiceBus(string connection)
    {
        var subscriptionsBySubjectName = new Dictionary<string, string>
            {
              { nameof(EmailToSend), Constants.EmailSubscriptionName },
              { nameof(SmsToSend), Constants.SmsSubscriptionName },
              { nameof(TelegramToSend), Constants.TelegramSubscriptionName }
            };

        var admin = new ServiceBusAdministrationClient(connection);
        foreach (var name in subscriptionsBySubjectName)
        {
            try
            {
                await admin.CreateSubscriptionAsync(new CreateSubscriptionOptions(Constants.TopicName, name.Value),
                    new CreateRuleOptions
                    {
                        Filter = new CorrelationRuleFilter { Subject = name.Key },
                        Name = $"Filter-subject-name-{name.Key}"
                    });
            }
            catch
            {
                // TODO:  _log.LogInformation("Subscription with such name already exist!");
                // or/and _log.LogInformation("Topic with such rules already exist!");
            }
        }
    }
}