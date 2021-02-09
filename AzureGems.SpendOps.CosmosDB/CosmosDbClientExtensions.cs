using AzureGems.CosmosDB;
using System.Threading.Tasks;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class CosmosDbClientExtensions
	{
		public static async Task<ICosmosDbContainer> GetContainerForFeature(this ICosmosDbClient client, string containerId, string featureToTrack)
		{
			ICosmosDbContainer container = await client.GetContainer(containerId);
			TrackedCosmosDbContainer trackedContainer = container as TrackedCosmosDbContainer;
			if (trackedContainer != null)
			{
				trackedContainer.Feature = featureToTrack;
				return trackedContainer;
			}
			return container;
		}

		public static async Task<ICosmosDbContainer> GetContainerForFeature<TEntity>(this ICosmosDbClient client, string featureToTrack)
		{
			ICosmosDbContainer container = await client.GetContainer<TEntity>();
			TrackedCosmosDbContainer trackedContainer = container as TrackedCosmosDbContainer;
			if (trackedContainer != null)
			{
				trackedContainer.Feature = featureToTrack;
				return trackedContainer;
			}
			return container;
		}
	}
}
