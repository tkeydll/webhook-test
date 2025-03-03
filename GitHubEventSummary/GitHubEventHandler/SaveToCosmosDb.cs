using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GitHubEventHandler
{
    public class SaveToCosmosDb
    {
        private readonly ILogger<SaveToCosmosDb> _logger;

        private readonly string _cosmosDbConnectionString;

        public SaveToCosmosDb(ILogger<SaveToCosmosDb> logger)
        {
            _logger = logger;
            _cosmosDbConnectionString = Environment.GetEnvironmentVariable("COSMOS_DB_CONNECTION_STRING");
        }


        [Function("SaveToCosmosDb")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is empty");
            }

            dynamic data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
            {
                return new BadRequestObjectResult("Invalid JSON payload");
            }

            _logger.LogInformation($"Received data: {data}");

            CosmosClient cosmosClient = new CosmosClient(_cosmosDbConnectionString);
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("GitHub");
            Container container = await database.CreateContainerIfNotExistsAsync("Events", "/action");

            data.id = Guid.NewGuid().ToString();

            _logger.LogInformation($"Saving data to Cosmos DB: {data}");
            await container.CreateItemAsync(data);

            return new OkObjectResult("Data saved to Cosmos DB!");
        }
    }
}
