using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;

namespace daily_briefing_telegram_bot.Services.Google
{
    public class GoogleCalendarService : IGoogleCalendarService, IScoped
    {
        private readonly string jsonFileName;
        private readonly string CalendarId;

        public GoogleCalendarService(IConfiguration configuration)
        {
            jsonFileName = configuration.GetSection("Google").GetValue<string>("JsonFileName");
            CalendarId = configuration.GetSection("Google").GetValue<string>("CalendarId");
        }

        public async Task<Events> GetEvents()
        {
            var service = GetGoogleCalendarService();

            var listRequest = SetListRequestParameters(service);

            return await listRequest.ExecuteAsync();
        }

        private EventsResource.ListRequest SetListRequestParameters(CalendarService service)
        {
            var listRequest = service.Events.List(CalendarId);
            listRequest.TimeMin = DateTime.Now;
            listRequest.ShowDeleted = false;
            listRequest.SingleEvents = true;
            listRequest.MaxResults = 100;
            listRequest.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            return listRequest;
        }

        private CalendarService GetGoogleCalendarService()
        {
            var credentials = GetServiceAccountCredentials(jsonFileName);

            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = "Daily Briefing",
            });
        }

        private static ServiceAccountCredential GetServiceAccountCredentials(string jsonFile)
        {
            using var stream =
                new FileStream(jsonFile, FileMode.Open, FileAccess.Read);
            var configuration =
                global::Google.Apis.Json.NewtonsoftJsonSerializer.Instance.Deserialize<JsonCredentialParameters>(stream);
            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(configuration.ClientEmail)
                {
                    Scopes = new List<string>
                    {
                        CalendarService.Scope.Calendar
                    }
                }.FromPrivateKey(configuration.PrivateKey));
            return credential;
        }
    }
}
