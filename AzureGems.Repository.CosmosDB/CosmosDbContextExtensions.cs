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
		public static void AddCosmosDbContext<TContext>(this IServiceCollection services) where TContext : DbContext, new()
		{
			services.AddTransient(typeof(TContext), provider =>
			{
				var cosmosDbClient = provider.GetRequiredService<ICosmosDbClient>();
				var instance = new TContext();
				Type instanceType = typeof(TContext);

				// concrete repository type to instantiate against the IRepository<> interface
				Type repoType = typeof(CosmosDbContainerRepository<>);

				IEnumerable<PropertyInfo> contextRepositories = instanceType.GetProperties()
					.Where(prop =>
						prop.PropertyType.IsInterface &&
						prop.PropertyType.IsGenericType &&
						prop.PropertyType.GetGenericTypeDefinition() == typeof(IRepository<>));

				var containerFactory = provider.GetRequiredService<ICosmosDbContainerFactory>();
				
				foreach (PropertyInfo prop in contextRepositories)
				{
					Type repositoryEntityGenericType = prop.PropertyType.GetGenericArguments()[0];
					Type constructedRepoType = repoType.MakeGenericType(repositoryEntityGenericType);
					ContainerDefinition containerDefinition = cosmosDbClient.GetContainerDefinitionForType(prop.PropertyType.GetGenericArguments()[0]);

					ICosmosDbContainer container = containerFactory.Create(instanceType, containerDefinition, cosmosDbClient);

					var entityTypeNameResolverInstance = new CosmosDbEntityTypeNameResolver();
					var pkvResolver = new CosmosDbPartitionKeyResolver();

					var idValueGeneratorType = typeof(CosmosDbIdValueGenerator<>);
					var idValueGeneratorInstanceType = idValueGeneratorType.MakeGenericType(repositoryEntityGenericType);
					var idValueGeneratorInstance = Activator.CreateInstance(idValueGeneratorInstanceType);

					object repoInstance = Activator.CreateInstance(constructedRepoType, args: new object[] { container, entityTypeNameResolverInstance, idValueGeneratorInstance, pkvResolver });
					prop.SetValue(instance, repoInstance);
				}

				return instance;
			});
		}
	}
}
