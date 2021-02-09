using AzureGems.CosmosDB;
using System.Collections.Generic;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class CosmosDbResponseExtensions
	{
		public static CosmosDbChargedResponse ToChargedResponse(this CosmosDbResponse response,
			string containerId,
			string feature,
			IEnumerable<string> context)
		{
			return new CosmosDbChargedResponse()
			{
				ContainerId = containerId,
				Feature = feature,
				Tags = context,
				StatusCode = response.StatusCode,
				ExecutionTime = response.ExecutionTime,
				RequestCharge = response.RequestCharge
			};
		}
	}
}
