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
        public bool IsDeleted { get; set; }

        public Event(Google.Apis.Calendar.v3.Data.Event @event, bool longMultiDate = false)
        {
            Id = @event.Id;
            Summary = @event.Summary;
            LastOccurence = longMultiDate ? DateTimeOffset.Now.Date : DateTimeOffset.Parse(@event.Start.Date);
            Occurences = 1;
            Action = longMultiDate ? Action.Warning : Action.None;
            IsDeleted = false;
        }

        public void UpdateEvent(Google.Apis.Calendar.v3.Data.Event @event, bool longMultiDate = false)
        {
            Occurences++;
            Summary = @event.Summary;
            LastOccurence = longMultiDate ? DateTimeOffset.Now.Date : DateTimeOffset.Parse(@event.Start.Date);
            if (longMultiDate)
            {
                Action = Action.Warning;
            }
            else
            {
                Action = Occurences switch
                {
                    > 2 and < 5 => Action.Warning,
                    >= 5 => Action.Delete,
                    _ => Action
                };
            }
        }

        public bool OccuredOnTheSameDay(DateTimeOffset lastOccurence)
        {
            return LastOccurence == lastOccurence;
        }
        
        public Event()
        {
        }
    }
}