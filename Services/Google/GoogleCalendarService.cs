using System;
using System.Collections.Generic;
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
        private readonly string _calendarId;
        private readonly string _jsonFileName;

        public GoogleCalendarService(IConfiguration configuration)
        {
            _jsonFileName = configuration.GetSection("Google").GetValue<string>("JsonFileName");
            _calendarId = configuration.GetSection("Google").GetValue<string>("calendarId");
        }

        public async Task<Events> GetEvents(ExecutionContext context)
        {
            var service = GetGoogleCalendarService(context);

            var listRequest = SetListRequestParameters(service);

            return await listRequest.ExecuteAsync();
        }

        public async Task DeleteEvent(ExecutionContext context, string eventId)
        {
            var service = GetGoogleCalendarService(context);

            try
            {
                var response = await service.Events.Delete("alex1toryanik@gmail.com", eventId).ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private EventsResource.ListRequest SetListRequestParameters(CalendarService service)
        {
            var listRequest = service.Events.List(_calendarId);
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
                ApplicationName = "Daily Briefing"
            });
        }

        private static ServiceAccountCredential GetServiceAccountCredentials(string jsonFilePath)
        {
            using (var stream =
                new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read))
            {
                var configuration =
                    NewtonsoftJsonSerializer.Instance
                        .Deserialize<JsonCredentialParameters>(stream);
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
}