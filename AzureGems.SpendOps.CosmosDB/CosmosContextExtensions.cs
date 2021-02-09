using AzureGems.CosmosDB;
using AzureGems.Repository.Abstractions;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class CosmosContextExtensions
	{
		public static TCosmosContext ForFeature<TCosmosContext>(this TCosmosContext context, string feature) where TCosmosContext : CosmosContext
		{
			// find all the IRepositories, and if they have TrackedContainers, then set the feature
			IEnumerable<PropertyInfo> contextRepositories = typeof(TCosmosContext).GetProperties()
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
