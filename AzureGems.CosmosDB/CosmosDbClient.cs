using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace AzureGems.CosmosDB
{
	public class CosmosDbClient : ICosmosDbClient, IDisposable
	{
		// TODO: Swap to Dictionary<?????, ContainerDefinition>
		private readonly ContainerDefinition[] _containerDefinitions;

		private readonly CosmosClient _sdkClient;
		private readonly AsyncLazy<Database> _lazyDatabase;

		public IServiceProvider ServiceProvider { get; }

		private async Task<Database> GetDatabase()
		{
			return await _lazyDatabase.Value;
		}

		public ContainerDefinition GetContainerDefinition(string containerId)
		{
			ContainerDefinition containerDef = _containerDefinitions.FirstOrDefault(def => def.ContainerId == containerId);
			return containerDef;
		}

		public ContainerDefinition GetContainerDefinitionForType(Type t)
		{
			ContainerDefinition containerDefForT = _containerDefinitions.FirstOrDefault(def => def.EntityType == t);
			return containerDefForT;
		}

		public ContainerDefinition GetContainerDefinitionForType<T>()
		{
			Type t = typeof(T);
			return GetContainerDefinitionForType(t);
		}

		public CosmosDbClient(
			IServiceProvider serviceProvider,
			CosmosDbConnection connection,
			CosmosDbConfig dbConfig,
			IEnumerable<ContainerDefinition> containerDefinitions)
		{
			this.ServiceProvider = serviceProvider;

			IEnumerable<ContainerDefinition> definitions = containerDefinitions as ContainerDefinition[] ?? containerDefinitions.ToArray();
			_containerDefinitions = definitions.ToArray();

			_sdkClient = new CosmosClient(
				connection.EndPoint,
				connection.AuthKey,
				new CosmosClientOptions()
				{
					ConnectionMode = ConnectionMode.Direct,
					SerializerOptions = new CosmosSerializationOptions()
					{
						IgnoreNullValues = true,
						PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
					}
				});

			_lazyDatabase = new AsyncLazy<Database>(async () =>
			{
				DatabaseResponse resp = await _sdkClient.CreateDatabaseIfNotExistsAsync(dbConfig.DatabaseId, dbConfig.Throughput);

				foreach (ContainerDefinition containerDefinition in definitions)
				{
					await Internal_EnsureContainerExists(resp.Database, containerDefinition);
				}

				return resp.Database;
			});
		}

		private static async Task<Container> Internal_EnsureContainerExists(Database db, ContainerDefinition containerDefinition)
		{
			return await Internal_EnsureContainerExists(db, containerDefinition.ContainerId, containerDefinition.PartitionKeyPath, containerDefinition.Throughput);
		}

		private static async Task<Container> Internal_EnsureContainerExists(Database db, string containerId, string partitionKeyPath, int? throughput)
		{
			var containerDefinition = new ContainerProperties(id: containerId, partitionKeyPath: partitionKeyPath);

			ContainerResponse response = await db.CreateContainerIfNotExistsAsync(
				containerProperties: containerDefinition,
				throughput: throughput,
				requestOptions: null);

			return response.Container;
		}

		private async Task<Container> Internal_GetContainer(string containerId)
		{
			Database database = await GetDatabase();

			// TODO: Should we cache the containers instead of quering Cosmos each time
			Container container = database.GetContainer(containerId);
			return container;
		}

		public async Task<ICosmosDbContainer> GetContainer(string containerId)
		{
			Container container = await Internal_GetContainer(containerId);
			ContainerDefinition definition = GetContainerDefinition(containerId);
			return new CosmosDbContainer(definition, this, container);
		}

		public async Task<ICosmosDbContainer> GetContainer<TEntity>()
		{
			ContainerDefinition definition = GetContainerDefinitionForType<TEntity>();
			return await GetContainer(definition.ContainerId);
		}

		public void Dispose()
		{
			_sdkClient?.Dispose();
		}
	}
}