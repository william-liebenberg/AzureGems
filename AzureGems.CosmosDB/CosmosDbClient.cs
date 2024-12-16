using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace AzureGems.CosmosDB
{
	public class CosmosDbClient : ICosmosDbClient, IDisposable
	{
		private readonly List<ContainerDefinition> _containerDefinitions = [];
		
		private readonly CosmosClient _sdkClient;
		private Database? _database;
		private readonly CosmosDbDatabaseSettings _cosmosDbConfig;
		
		private async Task<Database> GetDatabase()
		{
			return _database ??= await Internal_CreateDatabase();
		}

		private readonly ICosmosDbContainerFactory? _containerFactory;

		private readonly ConcurrentDictionary<string, ICosmosDbContainer> _containerCache = [];

		public IEnumerable<ContainerDefinition> ContainerDefinitions => _containerDefinitions;

		public void AddContainerDefinition(ContainerDefinition containerDefinition)
		{
			// TODO: WHAT IS GOING ON HERE -- Refactor
			
			ContainerDefinition? existing = GetContainerDefinition(containerDefinition.ContainerId);
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
				////ContainerDefinition definition = GetContainerDefinitionForType(containerDefinition.EntityType);
				
				//ContainerDefinition definition = GetContainerDefinition(containerDefinition.ContainerId);
				var definition = GetContainerDefinition(id);
				if (definition is null)
				{
					throw new Exception($"Container definition [{containerDefinition.ContainerId}] not found!");
				}

				// Container cosmosSdkContainer = await Internal_GetContainer(containerDefinition.ContainerId);
				//Container cosmosSdkContainer = await Internal_EnsureContainerExists(await this.GetDatabase(), containerDefinition);
				var cosmosSdkDatabase = await GetDatabase();
				var cosmosSdkContainer = await Internal_EnsureContainerExists(cosmosSdkDatabase, definition);
				//CosmosDbContainer container = new (definition, this, cosmosSdkContainer);
				var container = new CosmosDbContainer(definition, cosmosSdkContainer);

				// if a container factory is configured, use the container factory to allow for custom container implementations (such as tracking request unit charges),
				// otherwise return the default container
				
				// TODO: replace factory name with something more appropriate - like postContainerCreateEvent
				return _containerFactory is null ? container : _containerFactory.Create(container);
			});
		}

		public ContainerDefinition? GetContainerDefinition(string containerId)
		{
			var containerDef = _containerDefinitions.FirstOrDefault(def => def.ContainerId == containerId);
			return containerDef;
		}

		public ContainerDefinition? GetContainerDefinitionForType(Type t)
		{
			var containerDefForT = _containerDefinitions.FirstOrDefault(def => def.EntityType == t);
			return containerDefForT;
		}

		public CosmosDbClient(
			CosmosDbConnectionSettings connectionSettings,
			CosmosDbDatabaseSettings cosmosDbConfig,
			ICosmosDbContainerFactory? containerFactory,
			IEnumerable<ContainerDefinition> containerDefinitions)
		{
			_containerFactory = containerFactory;

			IEnumerable<ContainerDefinition> definitions = containerDefinitions as ContainerDefinition[] ?? containerDefinitions.ToArray();
			_containerDefinitions.AddRange(definitions);
			
			_cosmosDbConfig = cosmosDbConfig;

			_sdkClient = new CosmosClient(
				connectionSettings.EndPoint,
				connectionSettings.AuthKey,
				new CosmosClientOptions()
				{
// currently this is a workaround for running CosmosDb Emulator without trusting the certificate
#if DEBUG
					HttpClientFactory = () =>
					{
						HttpMessageHandler httpMessageHandler = new HttpClientHandler()
						{
							ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
						};

						return new HttpClient(httpMessageHandler);
					},
#endif
					ConnectionMode = _cosmosDbConfig.ConnectionMode,
					SerializerOptions = new CosmosSerializationOptions()
					{
						IgnoreNullValues = true,
						PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
					}
				});
		}

		private async Task<Database> Internal_CreateDatabase()
		{
			Database? sdkDatabase;
			try
			{
				DatabaseResponse resp = await _sdkClient.CreateDatabaseIfNotExistsAsync(_cosmosDbConfig.DatabaseId, _cosmosDbConfig.SharedThroughput);
				sdkDatabase = resp.Database;
			}
			catch (CosmosException cx)
			{
				Debug.WriteLine(cx);
				throw;
			}
			
			foreach (ContainerDefinition containerDefinition in _containerDefinitions)
			{
				await Internal_EnsureContainerExists(sdkDatabase, containerDefinition);
			}

			return sdkDatabase;
		}

		private static async Task<Container> Internal_EnsureContainerExists(Database db, ContainerDefinition containerDefinition)
		{
			return await Internal_EnsureContainerExists(db, containerDefinition.ContainerId, containerDefinition.PartitionKeyPath, containerDefinition.Throughput);
		}

		private static async Task<Container> Internal_EnsureContainerExists(Database db, string containerId, string partitionKeyPath, int? throughput)
		{
			var containerDefinition = new ContainerProperties(id: containerId, partitionKeyPath: partitionKeyPath);

			// add some retry logic to endure the container is created successfully. each failed attempt should wait longer than the previous one
			int[] retryIntervals = [1000, 2000, 5000, 10000, 20000];
			ContainerResponse? response = null;
			foreach (int t in retryIntervals)
			{
				try
				{
					// Create the container if it does not exist
					response = await db.CreateContainerIfNotExistsAsync(
						containerProperties: containerDefinition,
						throughput: throughput);
					
					break;
				}
				catch (CosmosException ex)
				{
					Debug.WriteLine(ex.Message);
					if (ex.StatusCode != System.Net.HttpStatusCode.Conflict)
					{
						throw;
					}
					await Task.Delay(t);
				}
				catch (Exception ex)
				{
					// log exception
					Debug.WriteLine(ex.Message);
					throw;
				}
			}

			if (response != null) return response.Container;
			throw new Exception($"Failed to create container [{containerId}] after multiple attempts");
		}

		private async Task<Container> Internal_GetContainer(string containerId)
		{
			Database database = await GetDatabase();
			Container container = database.GetContainer(containerId);
			return container;
		}

		public async Task<ICosmosDbContainer?> GetContainer(string containerId)
		{
			// TODO: Avoid searching for container via ID, prefer type instead
			var definition = GetContainerDefinition(containerId);
			if (definition is null)
			{
				throw new Exception($"Container definition [{containerId}] not found!");
			}
			
			return await this.CreateContainer(definition);
		}

		public async Task<bool> DeleteContainer(ContainerDefinition containerDefinition)
		{
			Container sdkContainer = await Internal_GetContainer(containerDefinition.ContainerId);
			ContainerResponse sdkResponse = await sdkContainer.DeleteContainerAsync();
			CosmosDbResponse<ContainerProperties> deleteResponse = sdkResponse.ToCosmosDbResponse();
			if(!deleteResponse.IsSuccessful)
			{
				// TODO: need logging
				// TODO: throw exception?
				return false;
			}

			if(_containerCache.TryRemove(containerDefinition.ContainerId, out ICosmosDbContainer? removedContainer))
			{
				if (removedContainer is not null)
				{
					Debug.WriteLine($"Removed non-null container: [{removedContainer.Definition.ContainerId}]");
				}
				else
				{
					Debug.WriteLine($"Removed non-null container: [{containerDefinition.ContainerId}]");
				}

				// container was removed from cache successfully
				return true;
			}

			// container was not removed from cache...but do we really care?
			// TODO: do we care if cache entry is not removed properly?
			return true;
		}

		public void Dispose()
		{
			_sdkClient.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}