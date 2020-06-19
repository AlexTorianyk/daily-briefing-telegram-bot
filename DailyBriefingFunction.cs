using System;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Services.Google;
using daily_briefing_telegram_bot.Services.Telegram;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
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

        [FunctionName("DailyBriefingScheduled")]
        public async Task DailyBriefingScheduled([TimerTrigger("0 30 2 * * *")]TimerInfo myTimer, ExecutionContext context, ILogger log)
        {
            try
            {
                var events = await googleCalendarService.GetEvents(context);
                if (events.Items != null && events.Items.Count > 0)
                {
                    foreach (var item in events.Items)
                    {
                        if (string.IsNullOrEmpty(item.Start.Date) || string.IsNullOrEmpty(item.End.Date))
                        {
                            continue;
                        }
                        var itemStartDate = DateTime.Parse(item.Start.Date).Date;
                        var itemEndDate = DateTime.Parse(item.End.Date).Date;
                        var today = DateTime.Now.Date;

                        if (itemStartDate <= today && itemEndDate > today)
                        {
                            if (!string.IsNullOrEmpty(item.Summary))
                            {
                                await telegramService.SendMessage(item.Summary);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                throw;
            }
        }

        [FunctionName("DailyBriefingHttp")]
        public async Task DailyBriefingHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            try
            {
                var events = await googleCalendarService.GetEvents(context);
                if (events.Items != null && events.Items.Count > 0)
                {
                    foreach (var item in events.Items)
                    {
                        if (string.IsNullOrEmpty(item.Start.Date) || string.IsNullOrEmpty(item.End.Date))
                        {
                            continue;
                        }
                        var itemStartDate = DateTime.Parse(item.Start.Date).Date;
                        var itemEndDate = DateTime.Parse(item.End.Date).Date;
                        var today = DateTime.Now.Date;

                        if (itemStartDate <= today && itemEndDate > today)
                        {
                            if (!string.IsNullOrEmpty(item.Summary))
                            {
                                await telegramService.SendMessage(item.Summary);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
