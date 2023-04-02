using System;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Services;
using daily_briefing_telegram_bot.Services.Telegram;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace daily_briefing_telegram_bot
{
  public class GetRyanairFlightPrices
  {
    private readonly FlightService _flightService;
    private readonly ITelegramService _telegramService;

    public GetRyanairFlightPrices(FlightService flightService, ITelegramService telegramService)
    {
      _flightService = flightService;
      _telegramService = telegramService;
    }

    [FunctionName("GetRyanairFlightPrices")]
    public async Task Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ExecutionContext context,
        ILogger log)
    {
      try
      {
        var flightPrices = await _flightService.GetFlightPrices("WRO", "OPO", "2023-07-21", "2023-07-24");
        
        await _telegramService.SendMessageToFlightBot(flightPrices.destination_to_origin_trip[0][0].ToString());
        await _telegramService.SendMessageToFlightBot(flightPrices.origin_to_destination_trip[0][0].ToString());
      }
      catch (Exception e)
      {
        log.LogError(e, e.Message);
        throw;
      }
    }
  }
}
