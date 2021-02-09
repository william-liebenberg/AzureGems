using System;
using System.Threading.Tasks;

namespace AzureGems.CosmosDB
{
	public interface ICosmosDbClient
	{
		ContainerDefinition GetContainerDefinition(string containerId);
		ContainerDefinition GetContainerDefinitionForType<T>();
		ContainerDefinition GetContainerDefinitionForType(Type t);

		Task<ICosmosDbContainer> GetContainer(string containerId);
		Task<ICosmosDbContainer> GetContainer<TEntity>();

		Task<ICosmosDbContainer> CreateContainer(ContainerDefinition containerDefinition);
	}
}