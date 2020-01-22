using Microsoft.Extensions.Configuration;

namespace AzureGems.CosmosDB
{
	public class CosmosDbConnection
	{
		public CosmosDbConnection(IConfiguration config)
		{
			EndPoint = config["cosmosDbConnection:endpoint"];
			AuthKey = config["cosmosDbConnection:authKey"];
		}
		
		public CosmosDbConnection(string endPoint, string authKey)
		{
			EndPoint = endPoint;
			AuthKey = authKey;
		}

		public string EndPoint { get; set; }
		public string AuthKey { get; set; }
	}

	public class CosmosDbConfig
	{
		public CosmosDbConfig(IConfiguration config)
		{
			DatabaseId = config["cosmosDbConnection:databaseId"];
			if (int.TryParse(config["cosmosDbConnection:throughput"], out int throughput))
			{
				Throughput = throughput;
			}
		}

		public CosmosDbConfig(string databaseId, int? throughput)
		{
			DatabaseId = databaseId;
			Throughput = throughput;
		}

		public string DatabaseId { get; set; }
		public int? Throughput { get; set; } = 400;
	}
}