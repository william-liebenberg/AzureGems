namespace AzureGems.Repository.CosmosDB
{
	public class CosmosDbEntityTypeNameResolver : IEntityTypeNameResolver
	{
		public string ResolveEntityTypeName<TEntity>()
		{
			return typeof(TEntity).Name;
		}
	}
}