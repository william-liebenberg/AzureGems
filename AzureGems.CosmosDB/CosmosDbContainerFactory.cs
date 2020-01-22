using System;

namespace AzureGems.CosmosDB
{
	public class CosmosDbContainerFactory : ICosmosDbContainerFactory
	{
		public Func<Type, IContainerDefinition, ICosmosDbClient, ICosmosDbContainer> Provider { get; set; }

		public ICosmosDbContainer Create(Type creatorType, IContainerDefinition definition, ICosmosDbClient client)
		{
			if (Provider == null)
			{
				ICosmosDbContainer container = client
						.GetContainer(definition.ContainerId)
						.ConfigureAwait(false).GetAwaiter().GetResult();
				return container;
			}

			return Provider(creatorType, definition, client);
		}
	}
}