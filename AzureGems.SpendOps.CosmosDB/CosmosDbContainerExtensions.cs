using AzureGems.CosmosDB;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class CosmosDbContainerExtensions
	{
		public static ICosmosDbContainer Tag(this ICosmosDbContainer container, string context)
		{
			TrackedCosmosDbContainer trackedContainer = container as TrackedCosmosDbContainer;
			if (trackedContainer != null)
			{
				trackedContainer.Tags.Clear();
				trackedContainer.Tags.Add(context);
				return trackedContainer;
			}
			return container;
		}

		public static ICosmosDbContainer AddTag(this ICosmosDbContainer container, string context)
		{
			TrackedCosmosDbContainer trackedContainer = container as TrackedCosmosDbContainer;
			if (trackedContainer != null)
			{
				trackedContainer.Tags.Add(context);
				return trackedContainer;
			}
			return container;
		}
	}
}
