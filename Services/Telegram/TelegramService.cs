using System.Threading.Tasks;
using daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace daily_briefing_telegram_bot.Services.Telegram
{
    public class TelegramService : ITelegramService, IScoped
    {
        private readonly long ChatId;
        private readonly string BotToken;

        public TelegramService(IConfiguration configuration)
        {
            ChatId = configuration.GetSection("Telegram").GetValue<int>("ChatId");
            BotToken = configuration.GetSection("Telegram").GetValue<string>("BotToken");
        }

        public async Task SendMessage(string message)
        {
            var botClient = new TelegramBotClient(BotToken);
            await botClient.SendTextMessageAsync(ChatId, message);
        }
    }
}
