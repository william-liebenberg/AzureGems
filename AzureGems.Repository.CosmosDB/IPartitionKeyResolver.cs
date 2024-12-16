using AzureGems.Repository.Abstractions;

namespace AzureGems.Repository.CosmosDB
{
	public interface IPartitionKeyResolver
	{
		string ResolvePartitionKeyValue(string partitionKeyPath, BaseEntity entityInstance);
	}
}