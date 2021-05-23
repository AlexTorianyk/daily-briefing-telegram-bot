using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Json;
using Google.Apis.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace daily_briefing_telegram_bot.Services.Google
{
    public class GoogleCalendarService : IGoogleCalendarService, IScoped
    {
        private readonly string _jsonFileName;
        private readonly string _applicationName;
        private readonly IEnumerable<string> _calendarIds;

        public GoogleCalendarService(IConfiguration configuration)
        {
            _jsonFileName = configuration.GetSection("Google").GetValue<string>("JsonFileName");
            _applicationName = configuration.GetSection("Google").GetValue<string>("ApplicationName");
            _calendarIds = InitializeCalendarIds(configuration);
        }

        private static IEnumerable<string> InitializeCalendarIds(IConfiguration configuration)
        {
            var primaryCalendarId = configuration.GetSection("Google").GetSection("Calendars")
                .GetValue<string>("Primary");
            var workCalendarId = configuration.GetSection("Google").GetSection("Calendars")
                .GetValue<string>("Work");
            
            return new Collection<string> {primaryCalendarId, workCalendarId};
        }

        public async Task<Events> GetEvents(ExecutionContext context, string calendarId = "primary")
        {
            var service = GetGoogleCalendarService(context);

            var listRequest = SetListRequestParameters(service, calendarId);

            return await listRequest.ExecuteAsync();
        }

        public async Task DeleteEvent(ExecutionContext context, string eventId, string calendarId = "primary")
        {
            var service = GetGoogleCalendarService(context);

            await service.Events.Delete(calendarId, eventId).ExecuteAsync();
        }

        private static EventsResource.ListRequest SetListRequestParameters(CalendarService service, string calendarId)
        {
            var listRequest = service.Events.List(calendarId);
            listRequest.TimeMin = DateTime.Now;
            listRequest.ShowDeleted = false;
            listRequest.SingleEvents = true;
            listRequest.MaxResults = 100;
            listRequest.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            
            return listRequest;
        }

        private CalendarService GetGoogleCalendarService(ExecutionContext context)
        {
            var credentials = GetServiceAccountCredentials(Path.Combine(context.FunctionAppDirectory, _jsonFileName));

            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = _applicationName
            });
        }

        private static ServiceAccountCredential GetServiceAccountCredentials(string jsonFilePath)
        {
            using var stream =
                new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
            
            var configuration =
                NewtonsoftJsonSerializer.Instance
                    .Deserialize<JsonCredentialParameters>(stream);
            
            return new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(configuration.ClientEmail)
                {
                    Scopes = new List<string>
                    {
                        CalendarService.Scope.Calendar
                    }
                }.FromPrivateKey(configuration.PrivateKey));
        }

        public IEnumerable<string> GetCalendarIds()
        {
            return _calendarIds;
        }
    }
}