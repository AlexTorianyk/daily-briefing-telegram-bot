using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using daily_briefing_telegram_bot.Extensions.AutomaticDependencyInjection;
using daily_briefing_telegram_bot.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace daily_briefing_telegram_bot.Persistence
{
    public class EventRepository : IScoped
    {
        private readonly Container _container;

        public EventRepository(CosmosClient client, IConfiguration configuration)
        {
            _container = client.GetContainer(configuration.GetSection("CosmosDb").GetValue<string>("DatabaseId"),
                configuration.GetSection("CosmosDb").GetValue<string>("EventContainer"));
        }

        public async Task Upsert(Event @event)
        {
            await _container.UpsertItemAsync(@event);
        }

        public IEnumerable<Event> LoadAll()
        {
            return _container.GetItemLinqQueryable<Event>(true).ToList();
        }
    }
}