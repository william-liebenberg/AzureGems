using System;
using Microsoft.Extensions.Configuration;

namespace AzureGems.CosmosDB
{
	public class CosmosDbConnectionSettings
	{
		public CosmosDbConnectionSettings(IConfiguration config)
		{
			EndPoint = config["cosmosDbConnection:endpoint"] ?? string.Empty;
			AuthKey = config["cosmosDbConnection:authKey"] ?? string.Empty;
		}
		
		public CosmosDbConnectionSettings(string endPoint, string authKey)
		{
			EndPoint = endPoint;
			AuthKey = authKey;
		}

		public string EndPoint { get; set; }
		public string AuthKey { get; set; }
	}
}