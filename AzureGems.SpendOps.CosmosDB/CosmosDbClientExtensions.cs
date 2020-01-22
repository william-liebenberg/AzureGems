using AzureGems.CosmosDB;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AzureGems.SpendOps.Abstractions;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class CosmosDbClientExtensions
	{
		public static async Task<ICosmosDbContainer> GetContainer<TEntity>(this ICosmosDbClient _client, string feature)
		{
			var chargeTracker = _client.ServiceProvider.GetService<IChargeTracker<CosmosDbChargedResponse>>();
			var container = await _client.GetContainer<TEntity>();
			return new TrackedCosmosDbContainer(container.Definition, container, chargeTracker, feature);
		}
	}
}
