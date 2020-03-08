using System;
using Microsoft.Azure.Cosmos;

namespace AzureGems.CosmosDB
{
	public static class CosmosResponseExtensions
	{
		public static CosmosDbResponse<T> ToCosmosDbResponse<T>(this Response<T> sdkResponse, TimeSpan? duration = null)
		{
			var res = new CosmosDbResponse<T>()
			{
				StatusCode = sdkResponse.StatusCode,

				RequestCharge = sdkResponse.RequestCharge,
				ExecutionTime = duration ?? TimeSpan.Zero,

				ActivityId = sdkResponse.ActivityId,
				ETag = sdkResponse.ETag,

				Result = sdkResponse.Resource,

				Diagnostics = sdkResponse.Diagnostics.ToString()
			};

			return res;
		}

		public static CosmosDbResponse<T> ToCosmosDbResponse<T>(this CosmosException sdkException, TimeSpan? duration = null)
		{
			var res = new CosmosDbResponse<T>()
			{
				Error = sdkException,

				StatusCode = sdkException.StatusCode,

				RequestCharge = sdkException.RequestCharge,
				ExecutionTime = duration ?? TimeSpan.Zero,

				ActivityId = sdkException.ActivityId,

				Diagnostics = sdkException.Diagnostics.ToString()
			};

			return res;
		}
	}
}
