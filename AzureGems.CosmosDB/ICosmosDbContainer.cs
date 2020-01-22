using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AzureGems.CosmosDB
{
	public interface ICosmosDbContainer
	{
		IContainerDefinition Definition { get; }
		ICosmosDbClient Client { get; }
				
		Task<CosmosDbResponse<T>> Add<T>(string partitionKey, T entity);
		Task<CosmosDbResponse<T>> Update<T>(string partitionKey, T entity);
		Task<CosmosDbResponse<T>> Delete<T>(string partitionKey, string id);
		Task<CosmosDbResponse<T>> Get<T>(string id);
		Task<CosmosDbResponse<T>> Get<T>(string partitionKey, string id);

		Task<CosmosDbResponse<IEnumerable<T>>> Get<T>(Expression<Func<T, bool>> predicate);
		Task<CosmosDbResponse<IEnumerable<T>>> Get<T>(string partitionKey, Expression<Func<T, bool>> predicate);

		Task<CosmosDbResponse<IEnumerable<T>>> GetByQuery<T>(string query);
		Task<CosmosDbResponse<IEnumerable<T>>> GetByQuery<T>(string partitionKey, string query);
		Task<CosmosDbResponse<IEnumerable<T>>> GetByQuery<T>(string partitionKey, string query, IReadOnlyDictionary<string, object> parameters);

		Task<CosmosDbResponse<IEnumerable<T>>> GetAll<T>();

		IQueryable<T> GetByLinq<T>();
		IQueryable<T> GetByLinq<T>(string partitionKey);
		Task<CosmosDbResponse<IEnumerable<T>>> Resolve<T>(IQueryable<T> query);
	}
}