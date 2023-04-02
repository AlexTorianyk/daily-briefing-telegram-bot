using System.Collections.Generic;

namespace daily_briefing_telegram_bot.Services
{
  public class FlightResponse
  {
    public List<List<Fare>> origin_to_destination_trip { get; set; }
    public List<List<Fare>> destination_to_origin_trip { get; set; }
  }

  public class Fare
  {
    public string currency { get; set; }
    public string destination_code { get; set; }
    public string origin_code { get; set; }
    public decimal regular_fare { get; set; }
    public int regular_fares_left { get; set; }

    public override string ToString()
    {
        return $"The flight from {origin_code} to {destination_code} is {regular_fare}{currency}. There are {regular_fares_left} seats left.";
    }
  }
}
