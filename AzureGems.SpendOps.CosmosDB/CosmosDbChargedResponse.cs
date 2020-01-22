using System;
using System.Collections.Generic;
using System.Net;

namespace AzureGems.SpendOps.CosmosDB
{
	public class CosmosDbChargedResponse
	{
		public string ContainerId { get; set; }
		public string Feature { get; set; }
		public IEnumerable<string> Context { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public TimeSpan ExecutionTime { get; set; }
		public double RequestCharge { get; set; }
	}

}
