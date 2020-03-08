using System;

namespace AzureGems.CosmosDB
{
	public class ContainerDefinition : IContainerDefinition
	{
		public ContainerDefinition(string containerId, string partitionKeyPath, Type entityType, bool queryByDiscriminator = true)
		{
			ContainerId = containerId;
			PartitionKeyPath = partitionKeyPath;
			EntityType = entityType;
			QueryByDiscriminator = queryByDiscriminator;
		}

		public ContainerDefinition(string containerId, string partitionKeyPath, Type entityType, int? throughput, bool queryByDiscriminator = true)
			: this(containerId, partitionKeyPath, entityType, queryByDiscriminator)
		{
			Throughput = throughput;
		}

		public Type EntityType { get; }
		public string ContainerId { get; }
		public string PartitionKeyPath { get; }
		public int? Throughput { get; }
		public bool QueryByDiscriminator { get; }
	}
}