using System;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace daily_briefing_telegram_bot
{
  public class GetRyanairFlightPrices
  {
    private readonly FlightService _flightService;
    public GetRyanairFlightPrices(FlightService flightService)
    {
      _flightService = flightService;
    }

    [FunctionName("GetRyanairFlightPrices")]
    public async Task Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ExecutionContext context,
        ILogger log)
    {
      try
      {
        var flightPrices = await _flightService.GetFlightPrices();
        log.LogInformation(flightPrices.destination_to_origin_trip[0][0].ToString());
        log.LogInformation(flightPrices.origin_to_destination_trip[0][0].ToString());
      }
      catch (Exception e)
      {
        log.LogError(e, e.Message);
        throw;
      }
    }
  }
}
