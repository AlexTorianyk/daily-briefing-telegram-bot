using System.Threading.Tasks;
using daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace daily_briefing_telegram_bot.Services.Telegram
{
    public class TelegramService : ITelegramService, IScoped
    {
        private readonly string _calendarBotToken;
        private readonly string _flightBotToken;
        private readonly long _chatId;

        public TelegramService(IConfiguration configuration)
        {
            _chatId = configuration.GetSection("Telegram").GetValue<int>("ChatId");
            _calendarBotToken = configuration.GetSection("Telegram").GetValue<string>("CalendarBotToken");
            _flightBotToken = configuration.GetSection("Telegram").GetValue<string>("FlightsBotToken");
        }

        public async Task SendMessageToCalendarBot(string message)
        {
            var botClient = new TelegramBotClient(_calendarBotToken);
            await botClient.SendTextMessageAsync(_chatId, message);
        }

        public async Task SendMessageToFlightBot(string message)
        {
            var botClient = new TelegramBotClient(_flightBotToken);
            await botClient.SendTextMessageAsync(_chatId, message);
        }
    }
}
