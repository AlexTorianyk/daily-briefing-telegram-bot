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
using Action = daily_briefing_telegram_bot.Models.Action;

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

        [FunctionName("PersistEventsScheduled")]
        public async Task PersistEventsScheduled([TimerTrigger("0 30 20 * * *")] TimerInfo myTimer,
            ExecutionContext context, ILogger log)
        {
            try
            {
                var events = await _googleCalendarService.GetEvents(context);
                if (events.Items is {Count: > 0})
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

        [FunctionName("PersistEventsHttp")]
        public async Task PersistEventsHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            try
            {
                var events = await _googleCalendarService.GetEvents(context);
                var savedEvents = _eventRepository.LoadAll();

                if (events.Items is {Count: > 0})
                    foreach (var item in events.Items)
                    {
                        var googleEvent = new GoogleEvent(item);

                        if (!googleEvent.HappenedToday()) continue;

                        var @event = savedEvents.SingleOrDefault(e => e.Id == item.Id);

                        if (@event == null)
                        {
                            await _eventRepository.Upsert(new Event(googleEvent));
                        }
                        else if (!@event.OccuredOn(googleEvent.StartDate))
                        {
                            @event.UpdateEvent(googleEvent);
                            await _eventRepository.Upsert(@event);
                        }
                    }
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                throw;
            }
        }

        [FunctionName("SendWarningsHttp")]
        public async Task SendWarningsHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            try
            {
                var events = _eventRepository.LoadAll();

                foreach (var @event in events)
                    if (@event.Action == Action.Warning)
                    {
                        await _telegramService.SendMessage(@event.Summary);

                        @event.ResetAction();
                        await _eventRepository.Upsert(@event);
                    }
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                throw;
            }
        }

        [FunctionName("SendWarningsScheduled")]
        public async Task SendWarningsScheduled(
            [TimerTrigger("0 30 21 * * *")] TimerInfo myTimer, ExecutionContext context,
            ILogger log)
        {
            try
            {
                var events = _eventRepository.LoadAll();

                foreach (var @event in events)
                    if (@event.Action == Action.Warning)
                    {
                        await _telegramService.SendMessage(@event.Summary);

                        @event.ResetAction();
                        await _eventRepository.Upsert(@event);
                    }
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                throw;
            }
        }

        [FunctionName("DeleteGoogleEventHttp")]
        public async Task DeleteGoogleEventHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            try
            {
                var events = _eventRepository.LoadAll();

                foreach (var @event in events)
                    if (@event.Action == Action.Delete && !@event.IsDeleted)
                    {
                        await _googleCalendarService.DeleteEvent(context, @event.Id);

                        @event.ResetAction();
                        @event.Delete();

                        await _eventRepository.Upsert(@event);
                    }
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                throw;
            }
        }

        [FunctionName("DeleteGoogleEventScheduled")]
        public async Task DeleteGoogleEventScheduled(
            [TimerTrigger("0 30 21 * * *")] TimerInfo myTimer, ExecutionContext context,
            ILogger log)
        {
            try
            {
                var events = _eventRepository.LoadAll();

                foreach (var @event in events)
                    if (@event.Action == Action.Delete && !@event.IsDeleted)
                    {
                        await _googleCalendarService.DeleteEvent(context, @event.Id);

                        @event.ResetAction();
                        @event.Delete();
                        
                        await _eventRepository.Upsert(@event);
                    }
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                throw;
            }
        }
    }
}