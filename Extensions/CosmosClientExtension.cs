using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace daily_briefing_telegram_bot.Extensions
{
    public static class ComsosClientExtension
    {
        public static void AddComsosClient(this IServiceCollection services, IConfiguration configuration)
        {
            var cosmosClient = new CosmosClient(
                configuration.GetValue<string>("ConnectionString"),
                new CosmosClientOptions
                {
                    ConnectionMode = ConnectionMode.Gateway,
                    SerializerOptions = new CosmosSerializationOptions
                        {PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase}
                });

            var task = Task.Run(async () =>
            {
                var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(configuration
                    .GetValue<string>("DatabaseId"));
                await databaseResponse.Database.CreateContainerIfNotExistsAsync(
                    new ContainerProperties(configuration.GetValue<string>("EventContainer"), "/action"));
            });

            task.Wait();

            services.AddSingleton(cosmosClient);
        }
    }
}