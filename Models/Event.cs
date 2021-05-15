using System;

namespace daily_briefing_telegram_bot.Models
{
    public class Event
    {
        public string Id { get; set; }
        public string Summary { get; set; }
        public DateTimeOffset LastOccurence { get; set; }
        public int Occurences { get; set; }
        public Action Action { get; set; }

        public Event(Google.Apis.Calendar.v3.Data.Event @event)
        {
            Id = @event.Id;
            Summary = @event.Summary;
            LastOccurence = DateTimeOffset.Parse(@event.Start.Date);
            Occurences = 1;
            Action = Action.None;
        }

        public void UpdateEvent(Google.Apis.Calendar.v3.Data.Event @event)
        {
            Occurences++;
            Summary = @event.Summary;
            LastOccurence = DateTimeOffset.Parse(@event.Start.Date);
            Action = Occurences switch
            {
                > 2 and < 5 => Action.Warning,
                >= 5 => Action.Delete,
                _ => Action
            };
        }
        
        public Event()
        {
        }
    }
}