using System;
using System.Linq;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Models;
using daily_briefing_telegram_bot.Persistence;
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
        private readonly EventRepository _eventRepository;
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly ITelegramService _telegramService;

        public DailyBriefingFunction(IGoogleCalendarService googleCalendarService, ITelegramService telegramService,
            EventRepository eventRepository)
        {
            _googleCalendarService = googleCalendarService;
            _telegramService = telegramService;
            _eventRepository = eventRepository;
        }

        [FunctionName("DailyBriefingScheduled")]
        public async Task DailyBriefingScheduled([TimerTrigger("0 30 2 * * *")] TimerInfo myTimer,
            ExecutionContext context, ILogger log)
        {
            try
            {
                var events = await _googleCalendarService.GetEvents(context);
                if (events.Items != null && events.Items.Count > 0)
                    foreach (var item in events.Items)
                    {
                        if (string.IsNullOrEmpty(item.Start.Date) || string.IsNullOrEmpty(item.End.Date)) continue;
                        var itemStartDate = DateTime.Parse(item.Start.Date).Date;
                        var itemEndDate = DateTime.Parse(item.End.Date).Date;
                        var today = DateTime.Now.Date;

                        if (itemStartDate <= today && itemEndDate > today)
                            if (!string.IsNullOrEmpty(item.Summary))
                                await _telegramService.SendMessage(item.Summary);
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
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            try
            {
                var events = await _googleCalendarService.GetEvents(context);
                var savedEvents = _eventRepository.LoadAll();

                if (events.Items is {Count: > 0})
                    foreach (var googleEvent in events.Items)
                    {
                        var googleEventStartDate = GetGoogleEventStartDate(googleEvent);
                        var googleEventEndDate = GetGoogleEventEndDate(googleEvent);
                     
                        if (!GoogleEventHappenedToday(googleEventStartDate, googleEventEndDate)) continue;
                        
                        if (IsMultiDayGoogleEvent(googleEventEndDate, googleEventStartDate)) continue;

                        var @event = savedEvents.SingleOrDefault(e => e.Id == googleEvent.Id);

                        if (@event == null)
                        {
                            await _eventRepository.Upsert(new Event(googleEvent));
                        }
                        else if (!@event.OccuredOnTheSameDay(googleEventStartDate))
                        {
                            @event.UpdateEvent(googleEvent);
                            await _eventRepository.Upsert(@event);
                        }
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static DateTime GetGoogleEventStartDate(Google.Apis.Calendar.v3.Data.Event googleEvent)
        {
            return DateTime.Parse(googleEvent.Start.Date).Date;
        }

        private static DateTime GetGoogleEventEndDate(Google.Apis.Calendar.v3.Data.Event googleEvent)
        {
            return DateTime.Parse(googleEvent.End.Date).Date;
        }

        private static bool GoogleEventHappenedToday(DateTime googleEventStartDate, DateTime googleEventEndDate)
        {
            return googleEventStartDate == DateTime.Now.Date || googleEventEndDate > DateTime.Now.Date;
        }

        private static bool IsMultiDayGoogleEvent(DateTime googleEventEndDate, DateTime googleEventStartDate)
        {
            return (googleEventEndDate - googleEventStartDate).TotalDays > 1;
        }
    }
}