using System;
using System.Collections.Generic;
using System.Net;

namespace AzureGems.SpendOps.CosmosDB
{
	public class CosmosDbChargedResponse
	{
		public string ContainerId { get; init; } = "";
		public string Feature { get; init; }
		public IEnumerable<string> Tags { get; init; }
		public HttpStatusCode StatusCode { get; init; }
		public TimeSpan ExecutionTime { get; init; }
		public double RequestCharge { get; init; }
	}

}
