using System;
using System.Net;

namespace AzureGems.CosmosDB
{
	public class CosmosDbResponse
	{
		public bool IsSuccessful => Error == null && ((int)StatusCode >= 200 && (int)StatusCode < 300);

		public Exception Error { get; set; }
		public string ErrorMessage => Error?.Message;

		public string ContinuationToken { get; set; }

		public HttpStatusCode StatusCode { get; set; }

		public double RequestCharge { get; set; }
		public TimeSpan ExecutionTime { get; set; }

		public string ActivityId { get; set; }
		public string ETag { get; set; }

		public string Diagnostics { get; set; }
	}

	public class CosmosDbResponse<T> : CosmosDbResponse
	{
		public T Result { get; set; }
	}
}