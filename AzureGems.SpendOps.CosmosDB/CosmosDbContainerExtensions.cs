using AzureGems.CosmosDB;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class CosmosDbContainerExtensions
	{
		public static ICosmosDbContainer Tag(this ICosmosDbContainer container, string context)
		{
			if (container is not TrackedCosmosDbContainer trackedContainer)
			{
				return container;
			}
			
			trackedContainer.Tags.Clear();
			trackedContainer.Tags.Add(context);
			
			return trackedContainer;
		}

		public static ICosmosDbContainer AddTag(this ICosmosDbContainer container, string context)
		{
			if (container is not TrackedCosmosDbContainer trackedContainer) return container;
			trackedContainer.Tags.Add(context);
			return trackedContainer;
		}
	}
}
