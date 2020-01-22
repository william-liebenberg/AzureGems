using AzureGems.Repository.Abstractions;

namespace AzureGems.Repository.CosmosDB
{
	public interface IIdValueGenerator<TEntity> where TEntity : BaseEntity
	{
		string Generate(TEntity e);
	}
}
