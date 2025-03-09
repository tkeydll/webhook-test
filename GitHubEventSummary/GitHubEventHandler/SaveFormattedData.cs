using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using GitHubEventHandler;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Company.Function
{
    public class SaveFormattedData
    {
        private readonly ILogger _logger;
        private readonly string _cosmosDbConnectionString;

        public SaveFormattedData(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SaveFormattedData>();
            _cosmosDbConnectionString = Environment.GetEnvironmentVariable("COSMOS_DB_CONNECTION_STRING");
        }

        [Function("SaveFormattedData")]
        public async Task<IActionResult> Run([CosmosDBTrigger(
            databaseName: "GitHub",
            containerName: "Events",
            Connection = "CosmosDBConnection",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] string input)
        {
            _logger.LogInformation("C# Cosmos DB trigger function processed a request.");
            // _logger.LogInformation($"Received data: {input}");

            var jsonArray = JArray.Parse(input);
            _logger.LogInformation($"Received data count: {jsonArray.Count}");

            foreach (var item in jsonArray)
            {
                _logger.LogInformation($"Raw data: {item}");

                var formatted = EventDataFormatter.FormatEventData(item.ToString());

                _logger.LogInformation("Formatted data: " + formatted);

                // add partition key
                var obj = JObject.Parse(formatted);
                obj.Add("date", DateTime.Now.ToString("yyyy-MM-dd"));

                // save cosmos db
                CosmosClient cosmosClient = new CosmosClient(_cosmosDbConnectionString);
                Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("GitHub");
                Container container = await database.CreateContainerIfNotExistsAsync("Formatted", "/date");

                await container.CreateItemAsync(obj);

                _logger.LogInformation("End: " + obj["id"]);
            }

            return new OkObjectResult("Data saved to Cosmos DB!");
        }
    }
}
