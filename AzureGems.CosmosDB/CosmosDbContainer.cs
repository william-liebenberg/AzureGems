using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

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

			ItemResponse<T> response = await _container.CreateItemAsync(entity, pk, new ItemRequestOptions()
			{
				IndexingDirective = IndexingDirective.Default
			});

			watch.Stop();

			CosmosDbResponse<T> result = response.ToCosmosDbResponse(watch.Elapsed);
			return result;
		}
		
		public async Task<CosmosDbResponse<T>> Update<T>(string partitionKey, T entity)
		{
			PartitionKey pk = PartitionKey.Null;
			if (!string.IsNullOrWhiteSpace(partitionKey))
			{
				pk = new PartitionKey(partitionKey);
			}

			Stopwatch watch = Stopwatch.StartNew();
			ItemResponse<T> response = await _container.UpsertItemAsync(entity, pk);
			watch.Stop();
			
			CosmosDbResponse<T> result = response.ToCosmosDbResponse(watch.Elapsed);
			return result;
		}

		public async Task<CosmosDbResponse<T>> Delete<T>(string partitionKey, string id)
		{
			PartitionKey pk = PartitionKey.None;
			if (!string.IsNullOrWhiteSpace(partitionKey))
			{
				pk = new PartitionKey(partitionKey);
			}

			Stopwatch watch = Stopwatch.StartNew();
			ItemResponse<T> response = await _container.DeleteItemAsync<T>(id, pk);
			watch.Stop();
			
			CosmosDbResponse<T> result = response.ToCosmosDbResponse(watch.Elapsed);
			return result;
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
			ItemResponse<T> response = await _container.ReadItemAsync<T>(id, pk);
			watch.Stop();

			CosmosDbResponse<T> result = response.ToCosmosDbResponse(watch.Elapsed);
			return result;
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
			var result = new CosmosDbResponse<IEnumerable<T>>();
			var results = new List<T>();

			Stopwatch watch = Stopwatch.StartNew();

			FeedIterator<T> resultSet = _container.GetItemQueryIterator<T>();
			while (resultSet.HasMoreResults)
			{
				FeedResponse<T> response = await resultSet.ReadNextAsync();
				results.AddRange(response);
				
				result.RequestCharge += response.RequestCharge;
				result.ActivityId = response.ActivityId;
				result.ETag = response.ETag;
			}

			watch.Stop();

			result.ExecutionTime = watch.Elapsed;
			result.Result = results;
			result.StatusCode = HttpStatusCode.OK;

			return result;
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

			var result = new CosmosDbResponse<IEnumerable<T>>();
			var dataResults = new List<T>();
			Stopwatch watch = Stopwatch.StartNew();

			FeedIterator<T> resultSetIterator = _container.GetItemQueryIterator<T>(queryDef, requestOptions: options);
			while (resultSetIterator.HasMoreResults)
			{
				FeedResponse<T> response = await resultSetIterator.ReadNextAsync();
				dataResults.AddRange(response);

				result.RequestCharge += response.RequestCharge;
				result.ActivityId = response.ActivityId;
				result.ETag = response.ETag;
			}

			watch.Stop();

			result.ExecutionTime = watch.Elapsed;
			result.Result = dataResults;
			result.StatusCode = HttpStatusCode.OK;

			return result;
		}

		public IQueryable<T> GetByLinq<T>()
		{
			PartitionKey pk = PartitionKey.Null;

			var options = new QueryRequestOptions
			{
				PartitionKey = pk
			};

			return _container.GetItemLinqQueryable<T>(
				allowSynchronousQueryExecution: true,
				continuationToken: null,
				requestOptions: options)
				.WithDiscriminator(Definition.EntityType.Name);
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

			return _container.GetItemLinqQueryable<T>(
				allowSynchronousQueryExecution: true,
				continuationToken: null,
				requestOptions: options)
				.WithDiscriminator(Definition.EntityType.Name);
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> Resolve<T>(IQueryable<T> query)
		{
			var result = new CosmosDbResponse<IEnumerable<T>>();
			var results = new List<T>();

			Stopwatch watch = Stopwatch.StartNew();

			// FeedIterator<T> iterator = query.ToFeedIterator(); // << NOT WORKING!!

			QueryDefinition queryDef = query.ToQueryDefinition();
			FeedIterator<T> iterator = _container.GetItemQueryIterator<T>(queryDef);

			while (iterator.HasMoreResults)
			{
				FeedResponse<T> response = await iterator.ReadNextAsync();
				results.AddRange(response);

				result.RequestCharge += response.RequestCharge;
				result.ActivityId = response.ActivityId;
				result.ETag = response.ETag;
			}

			watch.Stop();

			result.ExecutionTime = watch.Elapsed;
			result.Result = results;
			result.StatusCode = HttpStatusCode.OK;

			return result;
		}
	}
}