using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;

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

			if(Enum.TryParse<ConnectionMode>(config["cosmosDbConnection:mode"], out ConnectionMode mode))
			{
				ConnectionMode = mode;
			}
		}

		public CosmosDbDatabaseSettings(string databaseId, int? sharedThroughput, ConnectionMode mode)
		{
			DatabaseId = databaseId;
			SharedThroughput = sharedThroughput;
			ConnectionMode = mode;
		}

		public string DatabaseId { get; set; }
		public int? SharedThroughput { get; set; } = 400;
		public ConnectionMode ConnectionMode { get; set; } = ConnectionMode.Gateway;
	}
}