using AzureGems.CosmosDB;
using AzureGems.Repository.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
						prop.PropertyType is { IsInterface: true, IsGenericType: true } &&
						prop.PropertyType.GetGenericTypeDefinition() == typeof(IRepository<>));

				foreach (PropertyInfo prop in contextRepositories)
				{
					Type repositoryEntityGenericType = prop.PropertyType.GetGenericArguments()[0];
					Type constructedRepoType = repoType.MakeGenericType(repositoryEntityGenericType);

					// Question: how can we allow multiple repos of the same type but with different names and pk paths?
					// We somehow have to search via ContainerDefs or... rework the whole config story...and have a Config() method on the CosmosContext that we use to configure each repository individually...
					// Answer: Having multiple repositories for the same type in a single DB Context will require some kind of keyed configuration.
					// Answer 2: Use different CosmosContexts to separate the repositories
					
					Type entityType = prop.PropertyType.GetGenericArguments()[0];
					ContainerDefinition? containerDefinition = cosmosDbClient.GetContainerDefinitionForType(entityType);

					if (containerDefinition is null)
					{
						throw new Exception($"Container Definition for type [{entityType.Name}] not found!");
					}
					
					ICosmosDbContainer container = cosmosDbClient.CreateContainer(containerDefinition).ConfigureAwait(false).GetAwaiter().GetResult();

					var entityTypeNameResolverInstance = new CosmosDbEntityTypeNameResolver();
					var pkvResolver = new CosmosDbPartitionKeyResolver();

					Type idValueGeneratorType = typeof(CosmosDbIdValueGenerator<>);
					Type idValueGeneratorInstanceType = idValueGeneratorType.MakeGenericType(repositoryEntityGenericType);
					object? idValueGeneratorInstance = Activator.CreateInstance(idValueGeneratorInstanceType);

					try
					{
						object? repoInstance = Activator.CreateInstance(constructedRepoType, args: [container, entityTypeNameResolverInstance, idValueGeneratorInstance, pkvResolver]);
						if (repoInstance is null)
						{
							throw new Exception($"Could not instantiate Container Repository for entity type: [{constructedRepoType.Name}]");
						}
						prop.SetValue(cosmosContext, repoInstance);
					}
					catch (Exception e)
					{
						Debug.WriteLine($"Could not instantiate Container Repository for entity type: [{constructedRepoType.Name}]");
						Debug.WriteLine(e.Message);
						throw;
					}
				}

				return cosmosContext;
			});
		}
	}
}
