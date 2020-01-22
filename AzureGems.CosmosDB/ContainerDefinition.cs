using System;

namespace AzureGems.CosmosDB
{
	public class ContainerDefinition : IContainerDefinition
	{
		public ContainerDefinition(string containerId, string partitionKeyPath, Type entityType)
		{
			ContainerId = containerId;
			PartitionKeyPath = partitionKeyPath;
			EntityType = entityType;
		}

		public ContainerDefinition(string containerId, string partitionKeyPath, Type entityType, int? throughput)
			: this(containerId, partitionKeyPath, entityType)
		{
			Throughput = throughput;
		}

		public Type EntityType { get; }
		public string ContainerId { get; }
		public string PartitionKeyPath { get; }
		public int? Throughput { get; }
	}
}