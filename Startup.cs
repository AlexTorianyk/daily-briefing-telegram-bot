using daily_briefing_telegram_bot;
using daily_briefing_telegram_bot.Extensions;
using daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace daily_briefing_telegram_bot
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDependencies();
            builder.Services.AddConfiguration();
        }
    }
}
