namespace AzureGems.CosmosDB
{
	public interface IContainerConfigBuilder
	{
		IContainerConfigBuilder AddContainer(ContainerDefinition containerDefinition);
		IContainerConfigBuilder AddContainer<T>(string containerId, string partitionKeyPath = "/id", int ? throughput = null, bool queryByDiscriminator = true);
	}
}