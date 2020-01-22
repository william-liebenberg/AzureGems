using AzureGems.Repository.Abstractions;
using AzureGems.Repository.CosmosDB;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class RepositoryExtensions
	{
		public static IRepository<TEntity> WithContext<TEntity>(this IRepository<TEntity> repo, string context)
			where TEntity : BaseEntity
		{
			if(repo is CosmosDbContainerRepository<TEntity> cosmosRepo)
			{
				if(cosmosRepo.Container is TrackedCosmosDbContainer trackedContainer)
				{
					trackedContainer.Context.Add(context);
				}
			}
			return repo;
		}
	}
}
