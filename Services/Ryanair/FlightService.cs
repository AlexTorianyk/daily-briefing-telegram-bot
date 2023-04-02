using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection;
using Microsoft.Extensions.Configuration;

namespace daily_briefing_telegram_bot.Services
{
  public class FlightService : ITransient
  {
    private readonly string _apiKey;

    public FlightService(IConfiguration configuration)
    {
      _apiKey = configuration.GetSection("Ryanair").GetValue<string>("APIKey");
    }

    public async Task<FlightResponse> GetFlightPrices(string originCode, string destinationCode, string originDepartureDate, string destinationDepartureDate)
    {
      var client = new HttpClient();
      
      var request = new HttpRequestMessage
      {
        Method = HttpMethod.Get,
        RequestUri = new Uri($"https://ryanair.p.rapidapi.com/flights?origin_code={originCode}&destination_code={destinationCode}&origin_departure_date={originDepartureDate}&destination_departure_date={destinationDepartureDate}"),
        Headers =
          {
            { "X-RapidAPI-Key", _apiKey },
            { "X-RapidAPI-Host", "ryanair.p.rapidapi.com" },
          },
      };

      using (var response = await client.SendAsync(request))
      {
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Console.WriteLine(body);

        var flightResponse = JsonSerializer.Deserialize<FlightResponse>(body);

        return flightResponse;
      }
    }
  }
}
