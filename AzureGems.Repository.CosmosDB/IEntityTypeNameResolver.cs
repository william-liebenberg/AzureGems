namespace AzureGems.Repository.CosmosDB
{
	public interface IEntityTypeNameResolver
	{
		string ResolveEntityTypeName<TEntity>();
	}
}
