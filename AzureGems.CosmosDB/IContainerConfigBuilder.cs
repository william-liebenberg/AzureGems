namespace AzureGems.CosmosDB
{
	public interface IContainerConfigBuilder
	{
		IContainerConfigBuilder AddContainer(ContainerDefinition containerDefinition);
		IContainerConfigBuilder AddContainer<T>(string containerId, string partitionKeyPath = "/pk", int? throughput = null);
	}
}