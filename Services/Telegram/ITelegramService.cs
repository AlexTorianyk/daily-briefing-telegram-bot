using System.Threading.Tasks;

namespace daily_briefing_telegram_bot.Services.Telegram
{
    public interface ITelegramService
    {
        Task SendMessageToCalendarBot(string message);
        Task SendMessageToFlightBot(string message);
    }
}
