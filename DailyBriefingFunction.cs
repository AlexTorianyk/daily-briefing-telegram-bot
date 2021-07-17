using System;
using System.Linq;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Models;
using daily_briefing_telegram_bot.Persistence;
using daily_briefing_telegram_bot.Services;
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
        private readonly FunctionHelperService _functionHelperService;

        public DailyBriefingFunction(FunctionHelperService functionHelperService)
        {
            _functionHelperService = functionHelperService;
        }

        [FunctionName("PersistCalendarEventsScheduled")]
        public async Task PersistCalendarEventsScheduled([TimerTrigger("0 30 20 * * *")] TimerInfo myTimer,
            ExecutionContext context, ILogger log)
        {
            try
            {
                await _functionHelperService.PersistCalendarEvents(context);
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                throw;
            }
        }

        [FunctionName("PersistCalendarEventsHttp")]
        public async Task PersistCalendarEventsHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ExecutionContext context,
            ILogger log)
        {
            try
            {
                await _functionHelperService.PersistCalendarEvents(context);
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
                await _functionHelperService.SendWarnings();
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
                await _functionHelperService.SendWarnings();
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
                await _functionHelperService.DeleteGoogleEvents(context);
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
                await _functionHelperService.DeleteGoogleEvents(context);
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);
                throw;
            }
        }
    }
}