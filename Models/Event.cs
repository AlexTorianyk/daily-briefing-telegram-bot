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

        public Event(GoogleEvent googleEvent)
        {
            Id = googleEvent.Id;
            Summary = googleEvent.Summary;
            LastOccurence = googleEvent.IsLongMultiDayEvent ? DateTimeOffset.Now.Date : googleEvent.StartDate;
            Occurences = 1;
            Action = googleEvent.IsLongMultiDayEvent ? Action.Warning : Action.None;
        }

        public Event()
        {
        }

        public void UpdateEvent(GoogleEvent googleEvent)
        {
            Occurences++;
            Summary = googleEvent.Summary;
            LastOccurence = googleEvent.IsLongMultiDayEvent ? DateTimeOffset.Now.Date : googleEvent.StartDate;
            if (googleEvent.IsLongMultiDayEvent)
                Action = Action.Warning;
            else
                Action = Occurences switch
                {
                    > 2 and < 5 => Action.Warning,
                    >= 5 => Action.Delete,
                    _ => Action
                };
        }

        public bool OccuredOn(DateTimeOffset date)
        {
            return LastOccurence == date;
        }

        public void ResetAction()
        {
            Action = Action.None;
        }
    }
}