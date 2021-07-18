using System;
using System.Linq;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection;
using daily_briefing_telegram_bot.Models;
using daily_briefing_telegram_bot.Persistence;
using daily_briefing_telegram_bot.Services.Google;
using daily_briefing_telegram_bot.Services.Telegram;
using Microsoft.Azure.WebJobs;
using Action = daily_briefing_telegram_bot.Models.Action;

namespace daily_briefing_telegram_bot.Services
{
    public class FunctionHelperService : ITransient
    {
        private readonly EventRepository _eventRepository;
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly ITelegramService _telegramService;

        public FunctionHelperService(IGoogleCalendarService googleCalendarService, ITelegramService telegramService,
            EventRepository eventRepository)
        {
            _googleCalendarService = googleCalendarService;
            _telegramService = telegramService;
            _eventRepository = eventRepository;
        }

        public async Task PersistCalendarEvents(ExecutionContext context)
        {
            var events = await _googleCalendarService.GetEvents(context);
            var savedEvents = _eventRepository.LoadAll();

            try
            {

           
                if (events.Items is {Count: > 0})
                    foreach (var item in events.Items)
                    {
                        var googleEvent = new GoogleEvent(item);

                        if (!googleEvent.HappenedToday()) continue;

                        var @event = savedEvents.FirstOrDefault(e => e.Id == item.Id);

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
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task SendWarnings()
        {
            var events = _eventRepository.LoadAll();

            foreach (var @event in events)
                if (@event.Action == Action.Warning && @event.LastOccurence == DateTimeOffset.Now.Date)
                {
                    await _telegramService.SendMessage($"{@event.Summary} {@event.Occurences} times? Are you for real?");
                }
        }

        public async Task DeleteGoogleEvents(ExecutionContext context)
        {
            var events = _eventRepository.LoadAll();

            foreach (var @event in events)
                if (@event.Action == Action.Delete && !@event.IsDeleted)
                {
                    await _telegramService.SendMessage($"{@event.Summary} has been deleted");
                    
                    await _googleCalendarService.DeleteEvent(context, @event.Id);

                    @event.Delete();
                    await _eventRepository.Upsert(@event);
                }
        }
    }
}