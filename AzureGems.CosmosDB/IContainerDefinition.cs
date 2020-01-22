using System;

namespace AzureGems.CosmosDB
{
	public interface IContainerDefinition
	{
		string ContainerId { get; }
		Type EntityType { get; }
		string PartitionKeyPath { get; }
		int? Throughput { get; }
	}
}