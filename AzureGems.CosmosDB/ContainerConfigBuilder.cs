using System.Collections.Generic;

namespace AzureGems.CosmosDB
{
	public class ContainerConfigBuilder : IContainerConfigBuilder
	{
		private readonly List<ContainerDefinition> _containerDefinitions = new List<ContainerDefinition>();

		public IContainerConfigBuilder AddContainer(ContainerDefinition containerDefinition)
		{
			_containerDefinitions.Add(containerDefinition);
			return this;
		}

		public IContainerConfigBuilder AddContainer<T>(string containerId, string partitionKeyPath, int? throughput)
		{
			_containerDefinitions.Add(
				new ContainerDefinition(containerId, partitionKeyPath, typeof(T), throughput)
				{});
			return this;
		}

		public IEnumerable<ContainerDefinition> Build()
		{
			return _containerDefinitions;
		}
	}
}