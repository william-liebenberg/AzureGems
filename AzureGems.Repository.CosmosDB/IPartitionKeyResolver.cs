namespace AzureGems.Repository.CosmosDB
{
	public interface IPartitionKeyResolver
	{
		string ResolvePartitionKeyValue(string partitionKeyPath, object entityInstance);
	}
}