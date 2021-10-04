using AzureGems.CosmosDB;
using AzureGems.Repository.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AzureGems.Repository.CosmosDB
{
	public static class CosmosDbContextExtensions
	{
		public static void AddCosmosContext<TContext>(this IServiceCollection services) where TContext : CosmosContext, new()
		{
			services.AddTransient(typeof(TContext), provider =>
			{
				var cosmosDbClient = provider.GetRequiredService<ICosmosDbClient>();
				var cosmosContext = new TContext();
				Type cosmosContextType = typeof(TContext);

				// concrete repository type to instantiate against the IRepository<> interface
				Type repoType = typeof(CosmosDbContainerRepository<>);

				IEnumerable<PropertyInfo> contextRepositories = cosmosContextType.GetProperties()
					.Where(prop =>
						prop.PropertyType.IsInterface &&
						prop.PropertyType.IsGenericType &&
						prop.PropertyType.GetGenericTypeDefinition() == typeof(IRepository<>));

				foreach (PropertyInfo prop in contextRepositories)
				{
					Type repositoryEntityGenericType = prop.PropertyType.GetGenericArguments()[0];
					Type constructedRepoType = repoType.MakeGenericType(repositoryEntityGenericType);

					// TODO: how can we allow multiple repos of the same type but with different names and pk paths?
					// We somehow have to search via ContainerDefs or... rework the whole config story...and have a Config() method on the CosmosContext that
					// we use to configure each repository individually...
					ContainerDefinition containerDefinition = cosmosDbClient.GetContainerDefinitionForType(prop.PropertyType.GetGenericArguments()[0]);

					ICosmosDbContainer container = cosmosDbClient.CreateContainer(containerDefinition).ConfigureAwait(false).GetAwaiter().GetResult();

					var entityTypeNameResolverInstance = new CosmosDbEntityTypeNameResolver();
					var pkvResolver = new CosmosDbPartitionKeyResolver();

					var idValueGeneratorType = typeof(CosmosDbIdValueGenerator<>);
					var idValueGeneratorInstanceType = idValueGeneratorType.MakeGenericType(repositoryEntityGenericType);
					var idValueGeneratorInstance = Activator.CreateInstance(idValueGeneratorInstanceType);

					object repoInstance = Activator.CreateInstance(constructedRepoType, args: new object[] { container, entityTypeNameResolverInstance, idValueGeneratorInstance, pkvResolver });
					prop.SetValue(cosmosContext, repoInstance);
				}

				return cosmosContext;
			});
		}
	}
}
