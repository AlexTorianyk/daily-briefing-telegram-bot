using System;
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
            builder.Services.AddConfiguration();
#if DEBUG
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json")
                .AddEnvironmentVariables()
                .Build();
#endif
            builder.Services.AddComsosClient(configuration.GetSection("CosmosDb"));
        }
    }
}