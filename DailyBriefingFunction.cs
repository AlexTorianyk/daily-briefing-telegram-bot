using System;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Services.Google;
using daily_briefing_telegram_bot.Services.Telegram;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace daily_briefing_telegram_bot
{
    public class DailyBriefingFunction
    {
        private readonly IGoogleCalendarService googleCalendarService;
        private readonly ITelegramService telegramService;

        public DailyBriefingFunction(IGoogleCalendarService googleCalendarService, ITelegramService telegramService)
        {
            this.googleCalendarService = googleCalendarService;
            this.telegramService = telegramService;
        }

        [FunctionName("DailyBriefing")]
        public async Task DailyBriefing([TimerTrigger("0 1 * * *")]TimerInfo myTimer, ILogger log)
        {
            var events = await googleCalendarService.GetEvents();
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var item in events.Items)
                {
                    var itemStartDate = DateTime.Parse(item.Start.Date).Date;
                    var itemEndDate = DateTime.Parse(item.End.Date).Date;
                    var today = DateTime.Now.Date;

                    if (itemStartDate <= today && itemEndDate > today)
                    {
                        await telegramService.SendMessage(item.Summary);
                    }
                }
            }
        }
    }
}
