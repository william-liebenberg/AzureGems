using Microsoft.Extensions.Configuration;

namespace AzureGems.CosmosDB
{
	public class CosmosDbConnectionSettings
	{
		public CosmosDbConnectionSettings(IConfiguration config)
		{
			EndPoint = config["cosmosDbConnection:endpoint"];
			AuthKey = config["cosmosDbConnection:authKey"];
		}
		
		public CosmosDbConnectionSettings(string endPoint, string authKey)
		{
			EndPoint = endPoint;
			AuthKey = authKey;
		}

		public string EndPoint { get; set; }
		public string AuthKey { get; set; }
	}

	public class CosmosDbDatabaseConfig
	{
		public CosmosDbDatabaseConfig(IConfiguration config)
		{
			DatabaseId = config["cosmosDbConnection:databaseId"];
			if (int.TryParse(config["cosmosDbConnection:sharedThroughput"], out int throughput))
			{
				SharedThroughput = throughput;
			}
		}

		public CosmosDbDatabaseConfig(string databaseId, int? sharedThroughput)
		{
			DatabaseId = databaseId;
			SharedThroughput = sharedThroughput;
		}

		public string DatabaseId { get; set; }
		public int? SharedThroughput { get; set; } = 400;
	}
}