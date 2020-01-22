using AzureGems.CosmosDB;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using AzureGems.SpendOps.Abstractions;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class CosmosDbContainerExtensions
	{
		public static ICosmosDbContainer WithContext(this ICosmosDbContainer container, string context)
		{
			if (container is TrackedCosmosDbContainer trackedCosmosDbContainer)
			{
				trackedCosmosDbContainer.Context.Add(context);
				return trackedCosmosDbContainer;
			}
			else
			{
				var chargeTracker = container.Client.ServiceProvider.GetRequiredService<IChargeTracker<CosmosDbChargedResponse>>();
				return new TrackedCosmosDbContainer(container.Definition, container, chargeTracker, "Global", new List<string> { context });
			}
		}
	}
}
