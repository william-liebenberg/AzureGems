using AzureGems.Repository.Abstractions;
using AzureGems.Repository.CosmosDB;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class RepositoryExtensions
	{
		public static IRepository<TEntity> Tag<TEntity>(this IRepository<TEntity> repo, string tag)
			where TEntity : BaseEntity
		{
			if (repo is not CosmosDbContainerRepository<TEntity> { Container: TrackedCosmosDbContainer trackedContainer })
			{
				return repo;
			}
			
			trackedContainer.Tags.Clear();
			trackedContainer.Tags.Add(tag);
			
			return repo;
		}

		public static IRepository<TEntity> AddTag<TEntity>(this IRepository<TEntity> repo, string tag)
			where TEntity : BaseEntity
		{
			if (repo is CosmosDbContainerRepository<TEntity> { Container: TrackedCosmosDbContainer trackedContainer })
			{
				trackedContainer.Tags.Add(tag);
			}
			return repo;
		}
	}
}
