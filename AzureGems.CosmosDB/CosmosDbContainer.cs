using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace AzureGems.CosmosDB
{
	public class CosmosDbContainer : ICosmosDbContainer
	{
		public IContainerDefinition Definition { get; }

		private readonly Container _container;

		public ICosmosDbClient Client { get; }

		public CosmosDbContainer(IContainerDefinition definition, ICosmosDbClient client, Container container)
		{
			Definition = definition;
			Client = client;
			_container = container;
		}

		public async Task<CosmosDbResponse<T>> Add<T>(string partitionKey, T entity)
		{
			PartitionKey pk = PartitionKey.Null;
			if (!string.IsNullOrWhiteSpace(partitionKey))
			{
				pk = new PartitionKey(partitionKey);
			}

			Stopwatch watch = Stopwatch.StartNew();

			try
			{
				ItemResponse<T> itemResponse = await _container.CreateItemAsync(entity, pk);
				watch.Stop();

				CosmosDbResponse<T> response = itemResponse.ToCosmosDbResponse(watch.Elapsed);
				return response;
			}
			catch (CosmosException cex)
			{
				watch.Stop();
				return cex.ToCosmosDbResponse<T>(watch.Elapsed);
			}
		}

		public async Task<CosmosDbResponse<T>> Update<T>(string partitionKey, T entity)
		{
			PartitionKey pk = PartitionKey.Null;
			if (!string.IsNullOrWhiteSpace(partitionKey))
			{
				pk = new PartitionKey(partitionKey);
			}

			Stopwatch watch = Stopwatch.StartNew();

			try
			{
				ItemResponse<T> itemResponse = await _container.UpsertItemAsync(entity, pk);
				watch.Stop();

				CosmosDbResponse<T> response = itemResponse.ToCosmosDbResponse(watch.Elapsed);
				return response;
			}
			catch (CosmosException cex)
			{
				watch.Stop();
				return cex.ToCosmosDbResponse<T>(watch.Elapsed);
			}
		}

		public async Task<CosmosDbResponse<T>> Delete<T>(string partitionKey, string id)
		{
			PartitionKey pk = PartitionKey.None;
			if (!string.IsNullOrWhiteSpace(partitionKey))
			{
				pk = new PartitionKey(partitionKey);
			}

			Stopwatch watch = Stopwatch.StartNew();

			try
			{
				ItemResponse<T> itemResponse = await _container.DeleteItemAsync<T>(id, pk);
				watch.Stop();

				CosmosDbResponse<T> response = itemResponse.ToCosmosDbResponse(watch.Elapsed);
				return response;
			}
			catch (CosmosException cex)
			{
				watch.Stop();
				return cex.ToCosmosDbResponse<T>(watch.Elapsed);
			}
		}

		public async Task<CosmosDbResponse<T>> Get<T>(string id)
		{
			return await Get<T>(null, id);
		}

		public async Task<CosmosDbResponse<T>> Get<T>(string partitionKey, string id)
		{
			PartitionKey pk = PartitionKey.Null;
			if (!string.IsNullOrWhiteSpace(partitionKey))
			{
				pk = new PartitionKey(partitionKey);
			}

			Stopwatch watch = Stopwatch.StartNew();

			try
			{
				ItemResponse<T> itemResponse = await _container.ReadItemAsync<T>(id, pk);
				watch.Stop();

				CosmosDbResponse<T> response = itemResponse.ToCosmosDbResponse(watch.Elapsed);
				return response;
			}
			catch (CosmosException cex)
			{
				watch.Stop();
				return cex.ToCosmosDbResponse<T>(watch.Elapsed);
			}
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> Get<T>(Expression<Func<T, bool>> predicate)
		{
			IQueryable<T> query = GetByLinq<T>().Where(predicate);
			return await Resolve(query);
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> Get<T>(string partitionKey, Expression<Func<T, bool>> predicate)
		{
			IQueryable<T> query = GetByLinq<T>(partitionKey).Where(predicate);
			return await Resolve(query);
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> GetAll<T>()
		{
			var response = new CosmosDbResponse<IEnumerable<T>>();
			var results = new List<T>();

			Stopwatch watch = Stopwatch.StartNew();

			try
			{
				FeedIterator<T> resultSet = _container.GetItemQueryIterator<T>();
				while (resultSet.HasMoreResults)
				{
					FeedResponse<T> feedResponse = await resultSet.ReadNextAsync();
					results.AddRange(feedResponse);

					response.RequestCharge += feedResponse.RequestCharge;
					response.ActivityId = feedResponse.ActivityId;
					response.ETag = feedResponse.ETag;
					response.Diagnostics = feedResponse.Diagnostics.ToString();
				}

				watch.Stop();

				response.ExecutionTime = watch.Elapsed;
				response.StatusCode = HttpStatusCode.OK;
			}
			catch (CosmosException cex)
			{
				watch.Stop();
				response.Error = cex;
				response.ActivityId = cex.ActivityId;
				response.StatusCode = cex.StatusCode;
				response.RequestCharge += cex.RequestCharge;
				response.Diagnostics = cex.Diagnostics.ToString();
			}
			finally
			{
				response.ExecutionTime = watch.Elapsed;
				response.Result = results;
			}

			return response;
		}
		
		public Task<CosmosDbResponse<IEnumerable<T>>> GetByQuery<T>(string query)
		{
			return GetByQuery<T>(null, query, null);
		}

		public Task<CosmosDbResponse<IEnumerable<T>>> GetByQuery<T>(string partitionKey, string query)
		{
			return GetByQuery<T>(partitionKey, query, null);
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> GetByQuery<T>(string partitionKey, string query, IReadOnlyDictionary<string, object> parameters)
		{
			var queryDef = new QueryDefinition(query);
			
			if (parameters != null)
			{
				foreach (KeyValuePair<string, object> kvp in parameters)
				{
					queryDef.WithParameter(kvp.Key, kvp.Value);
				}
			}

			var options = new QueryRequestOptions();

			if (!string.IsNullOrWhiteSpace(partitionKey))
			{
				options.PartitionKey = new PartitionKey(partitionKey);
			}

			var response = new CosmosDbResponse<IEnumerable<T>>();
			var results = new List<T>();
			Stopwatch watch = Stopwatch.StartNew();

			try
			{
				FeedIterator<T> resultSetIterator = _container.GetItemQueryIterator<T>(queryDef, requestOptions: options);
				while (resultSetIterator.HasMoreResults)
				{
					FeedResponse<T> feedResponse = await resultSetIterator.ReadNextAsync();
					results.AddRange(feedResponse);

					response.RequestCharge += feedResponse.RequestCharge;
					response.ActivityId = feedResponse.ActivityId;
					response.ETag = feedResponse.ETag;
					response.Diagnostics = feedResponse.Diagnostics.ToString();
				}

				watch.Stop();
				response.StatusCode = HttpStatusCode.OK;
			}
			catch (CosmosException cex)
			{
				watch.Stop();
				response.Error = cex;
				response.ActivityId = cex.ActivityId;
				response.StatusCode = cex.StatusCode;
				response.RequestCharge += cex.RequestCharge;
				response.Diagnostics = cex.Diagnostics.ToString();
			}
			finally
			{
				response.ExecutionTime = watch.Elapsed;
				response.Result = results;
			}

			return response;
		}

		public IQueryable<T> GetByLinq<T>()
		{
			PartitionKey pk = PartitionKey.Null;

			var options = new QueryRequestOptions
			{
				// EnableScanInQuery = true,
				PartitionKey = pk
			};

			IQueryable<T> query = _container.GetItemLinqQueryable<T>(
				allowSynchronousQueryExecution: true,
				continuationToken: null,
				requestOptions: options);

			if (Definition.QueryByDiscriminator)
			{
				query = query.WithDiscriminator(Definition.EntityType.Name);
			}

			return query;
		}

		public IQueryable<T> GetByLinq<T>(string partitionKey)
		{
			PartitionKey pk = PartitionKey.Null;
			if (!string.IsNullOrWhiteSpace(partitionKey))
			{
				pk = new PartitionKey(partitionKey);
			}

			var options = new QueryRequestOptions
			{
				PartitionKey = pk
			};

			IQueryable<T> query = _container.GetItemLinqQueryable<T>(
				allowSynchronousQueryExecution: true,
				continuationToken: null,
				requestOptions: options);

			if (Definition.QueryByDiscriminator)
			{
				query = query.WithDiscriminator(Definition.EntityType.Name);
			}

			return query;
		}
		
		/// <summary>
		/// use old fashioned string matching to rip out the select caluse and replace with a select count
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		private static string ConvertToCountQuery(string text)
		{
			int fromPosition = text.IndexOf("FROM", StringComparison.InvariantCulture);
			return "SELECT count(1) " + text.Substring(fromPosition);
		}


		public async Task<CosmosDbResponse<IEnumerable<T>>> Resolve<T>(IQueryable<T> query)
		{
			var response = new CosmosDbResponse<IEnumerable<T>>();
			var results = new List<T>();

			Stopwatch watch = Stopwatch.StartNew();
            try
			{
				QueryDefinition queryDef = query.ToQueryDefinition();
				FeedIterator<T> feedIterator = _container.GetItemQueryIterator<T>(queryDef);
				
				while (feedIterator.HasMoreResults)
				{
					FeedResponse<T> feedResponse = await feedIterator.ReadNextAsync();
					results.AddRange(feedResponse);
				
					response.RequestCharge += feedResponse.RequestCharge;
					response.ActivityId = feedResponse.ActivityId;
					response.ETag = feedResponse.ETag;
					response.Diagnostics = feedResponse.Diagnostics.ToString();
				}

				watch.Stop();
				response.StatusCode = HttpStatusCode.OK;
			}
			catch(CosmosException cex)
			{
				watch.Stop();
				response.Error = cex;
				response.ActivityId = cex.ActivityId;
				response.StatusCode = cex.StatusCode;
				response.RequestCharge += cex.RequestCharge;
				response.Diagnostics = cex.Diagnostics.ToString();
			}
			finally
			{
				response.ExecutionTime = watch.Elapsed;
				response.Result = results;
			}

			return response;
		}
	}
}