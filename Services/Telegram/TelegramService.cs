using System.Threading.Tasks;
using daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace daily_briefing_telegram_bot.Services.Telegram
{
    public class TelegramService : ITelegramService, IScoped
    {
        private readonly string _botToken;
        private readonly long _chatId;

        public TelegramService(IConfiguration configuration)
        {
            _chatId = configuration.GetSection("Telegram").GetValue<int>("ChatId");
            _botToken = configuration.GetSection("Telegram").GetValue<string>("BotToken");
        }

        public async Task SendMessage(string message)
        {
            var botClient = new TelegramBotClient(_botToken);
            await botClient.SendTextMessageAsync(_chatId, message);
        }
    }
}