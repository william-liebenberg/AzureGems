using AzureGems.CosmosDB;
using AzureGems.Repository.Abstractions;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class DbContextExtensions
	{
		public static TDbContext ForFeature<TDbContext>(this TDbContext context, string feature) where TDbContext : DbContext
		{
			// find all the IRepositories, and if they have TrackedContainers, then set the feature
			IEnumerable<PropertyInfo> contextRepositories = typeof(TDbContext).GetProperties()
				.Where(prop =>
					prop.PropertyType.IsInterface &&
					prop.PropertyType.IsGenericType &&
					prop.PropertyType.GetGenericTypeDefinition() == typeof(IRepository<>));

			foreach (var contextRepoProp in contextRepositories)
			{
				object repoValue = contextRepoProp.GetValue(context);
				PropertyInfo containerProp = repoValue.GetType()
					.GetProperties()
					.Where(r =>
						r.Name == "Container" &&
						r.PropertyType.IsInterface &&
						(r.PropertyType == typeof(ICosmosDbContainer) || r.PropertyType.IsSubclassOf(typeof(ICosmosDbContainer))))
					.FirstOrDefault();

				if (containerProp != null)
				{
					if (containerProp.GetValue(repoValue) is TrackedCosmosDbContainer trackedContainerValue)
					{
						trackedContainerValue.Feature = feature;
					}
				}
			}

			return context;
		}
	}
}
