using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace AzureGems.CosmosDB
{
	public class CosmosDbClient : ICosmosDbClient, IDisposable
	{
		private readonly List<ContainerDefinition> _containerDefinitions = new List<ContainerDefinition>();
		
		private readonly CosmosClient _sdkClient;
		private readonly AsyncLazy<Database> _lazyDatabase;

		private async Task<Database> GetDatabase()
		{
			return await _lazyDatabase.Value;
		}

		private readonly ICosmosDbContainerFactory _containerFactory;

		private readonly ConcurrentDictionary<string, ICosmosDbContainer> _containerCache = new ConcurrentDictionary<string, ICosmosDbContainer>();

		public IEnumerable<ContainerDefinition> ContainerDefinitions { get { return _containerDefinitions; } }

		public void AddContainerDefinition(ContainerDefinition containerDefinition)
		{
			ContainerDefinition existing = GetContainerDefinition(containerDefinition.ContainerId);
			//ContainerDefinition existing = GetContainerDefinitionForType(containerDefinition.EntityType);
			if (existing is null)
			{
				_containerDefinitions.Add(containerDefinition);
				return;
			}

			throw new NotImplementedException();
		}

		public async Task<ICosmosDbContainer> CreateContainer(ContainerDefinition containerDefinition)
		{
			return await _containerCache.GetOrAddAsync(containerDefinition.ContainerId, async id =>
			{
				//ContainerDefinition definition = GetContainerDefinitionForType(containerDefinition.EntityType);
				ContainerDefinition definition = GetContainerDefinition(containerDefinition.ContainerId);

				// Container cosmosSdkContainer = await Internal_GetContainer(containerDefinition.ContainerId);
				Container cosmosSdkContainer = await Internal_EnsureContainerExists(await this.GetDatabase(), containerDefinition);

				var container = new CosmosDbContainer(definition, this, cosmosSdkContainer);

				return _containerFactory == null ? container : _containerFactory.Create(container);
			});
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


		public CosmosDbClient(
			CosmosDbConnectionSettings connectionSettings,
			CosmosDbDatabaseSettings cosmosDbConfig,
			ICosmosDbContainerFactory containerFactory,
			IEnumerable<ContainerDefinition> containerDefinitions)
		{
			_containerFactory = containerFactory;

			IEnumerable<ContainerDefinition> definitions = containerDefinitions as ContainerDefinition[] ?? containerDefinitions.ToArray();
			_containerDefinitions.AddRange(definitions);

			_sdkClient = new CosmosClient(
				connectionSettings.EndPoint,
				connectionSettings.AuthKey,
				new CosmosClientOptions()
				{
					ConnectionMode = cosmosDbConfig.ConnectionMode,
					SerializerOptions = new CosmosSerializationOptions()
					{
						IgnoreNullValues = true,
						PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
					}
				});

			_lazyDatabase = new AsyncLazy<Database>(async () =>
			{
				DatabaseResponse resp = await _sdkClient.CreateDatabaseIfNotExistsAsync(cosmosDbConfig.DatabaseId, cosmosDbConfig.SharedThroughput);

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

            try
			{
				ContainerResponse response = await db.CreateContainerIfNotExistsAsync(
					containerProperties: containerDefinition,
					throughput: throughput,
					requestOptions: null);

				return response.Container;
			}
			catch(CosmosException cex)
			{
				throw;
			}
		}

		private async Task<Container> Internal_GetContainer(string containerId)
		{
			Database database = await GetDatabase();
			Container container = database.GetContainer(containerId);
			return container;
		}

		public async Task<ICosmosDbContainer> GetContainer(string containerId)
		{
			// TODO: Avoid searching for container via ID, prefer type instead
			ContainerDefinition definition = GetContainerDefinition(containerId);
			return await this.CreateContainer(definition);
		}

		public async Task<bool> DeleteContainer(ContainerDefinition containerDefinition)
		{
			Container sdkContainer = await Internal_GetContainer(containerDefinition.ContainerId);
			var sdkResponse = await sdkContainer.DeleteContainerAsync();
			var deleteResponse = sdkResponse.ToCosmosDbResponse();
			if(!deleteResponse.IsSuccessful)
			{
				// TODO: need logging
				// TODO: throw exception?
				return false;
			}
			
			if(_containerCache.TryRemove(containerDefinition.ContainerId, out ICosmosDbContainer removedContainer))
			{
				// container was removed from cache successfully
				return true;
			}

			// container was not removed from cache...but do we really care?
			// TODO: do we care if cache entry is not removed properly?
			return true;
		}

		public void Dispose()
		{
			_sdkClient?.Dispose();
		}
	}
}