using AzureGems.CosmosDB;
using AzureGems.SpendOps.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AzureGems.SpendOps.CosmosDB
{
	public interface IFeature
	{
		string Feature { get; set; }
	}

	public interface IRequestContext
	{
		ICollection<string> Context { get; }
	}

	public class TrackedCosmosDbContainer : ICosmosDbContainer, IFeature, IRequestContext
	{
		public IChargeTracker<CosmosDbChargedResponse> ChargeTracker { get; }
		public ICosmosDbClient Client => _innerContainer.Client;

		public string Feature { get; set;  }
		public ICollection<string> Context { get; }

		private ICosmosDbContainer _innerContainer { get; }
		public IContainerDefinition Definition { get; }

		public TrackedCosmosDbContainer(IContainerDefinition definition, ICosmosDbContainer container, IChargeTracker<CosmosDbChargedResponse> chargeTracker, string feature)
			: this(definition, container, chargeTracker, feature, new List<string>())
		{
		}

		public TrackedCosmosDbContainer(IContainerDefinition definition, ICosmosDbContainer container, IChargeTracker<CosmosDbChargedResponse> chargeTracker, string feature, ICollection<string> context)
		{
			Definition = definition;
			_innerContainer = container;
			ChargeTracker = chargeTracker;
			Feature = feature;
			Context = context;
			ChargeTracker = chargeTracker;
		}

		private async Task TrackResponse(CosmosDbResponse resp)
		{
			if (ChargeTracker != null)
			{
				await ChargeTracker?.Track(resp.ToChargedResponse(_innerContainer.Definition.ContainerId, Feature, Context));
			}
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> GetByQuery<T>(string partitionKey, string query)
		{
			CosmosDbResponse<IEnumerable<T>> resp = await _innerContainer.GetByQuery<T>(partitionKey, query);
			await TrackResponse(resp);
			return resp;
		}

		public async Task<CosmosDbResponse<T>> Add<T>(string partitionKey, T entity)
		{
			CosmosDbResponse<T> resp = await _innerContainer.Add(partitionKey, entity);
			await TrackResponse(resp);
			return resp;
		}

		public async Task<CosmosDbResponse<T>> Update<T>(string partitionKey, T entity)
		{
			CosmosDbResponse<T> resp = await _innerContainer.Update(partitionKey, entity);
			await TrackResponse(resp);
			return resp;
		}

		public async Task<CosmosDbResponse<T>> Delete<T>(string partitionKey, string id)
		{
			CosmosDbResponse<T> resp = await _innerContainer.Delete<T>(partitionKey, id);
			await TrackResponse(resp);
			return resp;
		}

		public async Task<CosmosDbResponse<T>> Get<T>(string id)
		{
			CosmosDbResponse<T> resp = await _innerContainer.Get<T>(id);
			await TrackResponse(resp);
			return resp;
		}

		public async Task<CosmosDbResponse<T>> Get<T>(string partitionKey, string id)
		{
			CosmosDbResponse<T> resp = await _innerContainer.Get<T>(partitionKey, id);
			await TrackResponse(resp);
			return resp;
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> Get<T>(Expression<Func<T, bool>> predicate)
		{
			CosmosDbResponse<IEnumerable<T>> resp = await _innerContainer.Get(predicate);
			await TrackResponse(resp);
			return resp;
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> Get<T>(string partitionKey, Expression<Func<T, bool>> predicate)
		{
			CosmosDbResponse<IEnumerable<T>> resp = await _innerContainer.Get(partitionKey, predicate);
			await TrackResponse(resp);
			return resp;
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> GetByQuery<T>(string query)
		{
			CosmosDbResponse<IEnumerable<T>> resp = await _innerContainer.GetByQuery<T>(query);
			await TrackResponse(resp);
			return resp;
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> GetByQuery<T>(string partitionKey, string query, IReadOnlyDictionary<string, object> parameters)
		{
			CosmosDbResponse<IEnumerable<T>> resp = await _innerContainer.GetByQuery<T>(partitionKey, query, parameters);
			await TrackResponse(resp);
			return resp;
		}

		public async Task<CosmosDbResponse<IEnumerable<T>>> GetAll<T>()
		{
			CosmosDbResponse<IEnumerable<T>> resp = await _innerContainer.GetAll<T>();
			await TrackResponse(resp);
			return resp;
		}

		public IQueryable<T> GetByLinq<T>() => _innerContainer.GetByLinq<T>();
		public IQueryable<T> GetByLinq<T>(string partitionKey) => _innerContainer.GetByLinq<T>(partitionKey);

		public async Task<CosmosDbResponse<IEnumerable<T>>> Resolve<T>(IQueryable<T> query)
		{
			CosmosDbResponse<IEnumerable<T>> resp = await _innerContainer.Resolve(query);
			await TrackResponse(resp);
			return resp;
		}
	}
}
