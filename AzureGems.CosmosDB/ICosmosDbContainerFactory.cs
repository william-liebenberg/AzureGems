using System;

namespace AzureGems.CosmosDB
{
	public interface ICosmosDbContainerFactory
	{
		Func<Type, IContainerDefinition, ICosmosDbClient, ICosmosDbContainer> Provider { get; set; }
		ICosmosDbContainer Create(Type creatorType, IContainerDefinition definition, ICosmosDbClient client);
	}
}