using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;

namespace daily_briefing_telegram_bot.Services.Google
{
    public interface IGoogleCalendarService
    {
        Task<Events> GetEvents();
    }
}
