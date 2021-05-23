using System;

namespace daily_briefing_telegram_bot.Models
{
    public class GoogleEvent
    {
        public string Id { get; }
        public string Summary { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public bool IsLongMultiDayEvent { get; }
        public string CalendarId { get; set; }

        public GoogleEvent(Google.Apis.Calendar.v3.Data.Event googleEvent, string calendarId)
        {
            Id = googleEvent.Id;
            Summary = googleEvent.Summary;
            StartDate = GetStartDate(googleEvent);
            EndDate = GetEndDate(googleEvent);
            IsLongMultiDayEvent = (EndDate - StartDate).TotalDays > 2;
            CalendarId = calendarId;
        }

        private static DateTime GetStartDate(Google.Apis.Calendar.v3.Data.Event googleEvent)
        {
            return googleEvent.Start.Date != null
                ? DateTime.Parse(googleEvent.Start.Date).Date
                : googleEvent.Start.DateTime.Value.Date;
        }

        private static DateTime GetEndDate(Google.Apis.Calendar.v3.Data.Event googleEvent)
        {
            return googleEvent.End.Date != null
                ? DateTime.Parse(googleEvent.End.Date).Date
                : googleEvent.End.DateTime.Value.Date;
        }

        public bool HappenedToday()
        {
            return StartDate == DateTime.Now.Date || EndDate == DateTime.Now.Date;
        }
    }
}