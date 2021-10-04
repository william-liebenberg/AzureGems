using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureGems.CosmosDB
{
	public interface ICosmosDbClient
	{
		IEnumerable<ContainerDefinition> ContainerDefinitions { get; }
		void AddContainerDefinition(ContainerDefinition containerDefinition);
		ContainerDefinition GetContainerDefinition(string containerId);
		ContainerDefinition GetContainerDefinitionForType(Type t);

		Task<ICosmosDbContainer> GetContainer(string containerId);

		Task<ICosmosDbContainer> CreateContainer(ContainerDefinition containerDefinition);

		Task<bool> DeleteContainer(ContainerDefinition containerDefinition);
	}
}