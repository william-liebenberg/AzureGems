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
					prop.PropertyType is { IsInterface: true, IsGenericType: true } &&
					prop.PropertyType.GetGenericTypeDefinition() == typeof(IRepository<>));

			foreach (PropertyInfo contextRepoProp in contextRepositories)
			{
				object repoValue = contextRepoProp.GetValue(context);

				if (repoValue is null)
				{
					continue;
				}
				
				PropertyInfo containerProp = repoValue
					.GetType()
					.GetProperties()
					.FirstOrDefault(r => r.Name == "Container" &&
					                     r.PropertyType.IsInterface &&
					                     (r.PropertyType == typeof(ICosmosDbContainer) || r.PropertyType.IsSubclassOf(typeof(ICosmosDbContainer))));

				if (containerProp == null)
				{
					continue;
				}
				
				if (containerProp.GetValue(repoValue) is TrackedCosmosDbContainer trackedContainerValue)
				{
					trackedContainerValue.Feature = feature;
				}
			}

			return context;
		}
	}
}
