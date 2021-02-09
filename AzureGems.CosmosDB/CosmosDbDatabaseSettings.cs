using Microsoft.Extensions.Configuration;

namespace AzureGems.CosmosDB
{
	public class CosmosDbDatabaseSettings
	{
		public CosmosDbDatabaseSettings(IConfiguration config)
		{
			DatabaseId = config["cosmosDbConnection:databaseId"];
			if (int.TryParse(config["cosmosDbConnection:sharedThroughput"], out int throughput))
			{
				SharedThroughput = throughput;
			}
		}

		public CosmosDbDatabaseSettings(string databaseId, int? sharedThroughput)
		{
			DatabaseId = databaseId;
			SharedThroughput = sharedThroughput;
		}

		public string DatabaseId { get; set; }
		public int? SharedThroughput { get; set; } = 400;
	}
}