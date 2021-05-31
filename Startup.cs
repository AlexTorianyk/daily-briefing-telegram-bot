using System.IO;
using daily_briefing_telegram_bot;
using daily_briefing_telegram_bot.Extensions;
using daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Startup))]

namespace daily_briefing_telegram_bot
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDependencies();
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddConfiguration(configuration);
            builder.Services.AddComsosClient(configuration.GetSection("CosmosDb"));
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), true, false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"),
                    true, false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "local.settings.json"), true, false)
                .AddEnvironmentVariables();
        }
    }
}