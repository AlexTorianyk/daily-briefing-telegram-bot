﻿using System.Threading.Tasks;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Azure.WebJobs;

namespace daily_briefing_telegram_bot.Services.Google
{
    public interface IGoogleCalendarService
    {
        Task<Events> GetEvents(ExecutionContext context);
        Task DeleteEvent(ExecutionContext context, string eventId);
    }
}