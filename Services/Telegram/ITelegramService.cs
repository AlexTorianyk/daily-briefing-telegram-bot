using System.Threading.Tasks;

namespace daily_briefing_telegram_bot.Services.Telegram
{
    public interface ITelegramService
    {
        Task SendMessage(string message);
    }
}
