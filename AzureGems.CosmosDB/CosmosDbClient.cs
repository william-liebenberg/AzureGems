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

		private readonly ConcurrentDictionary<Type, ICosmosDbContainer> _containerCache = [];

		public IEnumerable<ContainerDefinition> ContainerDefinitions => _containerDefinitions;

		public void AddContainerDefinition(ContainerDefinition containerDefinition)
		{
			ContainerDefinition? existing = GetContainerDefinitionForType(containerDefinition.EntityType);
			switch (existing)
			{
				case null:
				{
					_containerDefinitions.Add(containerDefinition);
					return;
				}
				default:
				{
					throw new ContainerDefinitionAlreadyExistsException(containerDefinition);
				}
			}
		}

		public async Task<ICosmosDbContainer> CreateContainer(ContainerDefinition containerDefinition)
		{
			// use GetOrAddAsync to ensure that the container is only created once - we are not using AddOrUpdate because we want to ensure that the container is only created once
			return await _containerCache.GetOrAddAsync(containerDefinition.EntityType, async type =>
			{
				ContainerDefinition? definition = GetContainerDefinitionForType(type);
				if (definition is null)
				{
					throw new ContainerDefinitionNotFoundException(type);
				}
				
				Database cosmosSdkDatabase = await GetDatabase();
				Container cosmosSdkContainer = await Internal_EnsureContainerExists(cosmosSdkDatabase, definition);
				var container = new CosmosDbContainer(definition, cosmosSdkContainer);
				
				// TODO: add preContainerCreateEvent
				
				// if a container factory is configured, use the container factory to allow for custom container implementations and/or configurations (such as tracking request unit charges),
				// otherwise return the default container
				return _containerFactory is null ? container : _containerFactory.Create(container);
				
				// TODO: add postContainerCreateEvent
			});
		}

		public ContainerDefinition? GetContainerDefinition(string containerId)
		{
			ContainerDefinition? containerDef = _containerDefinitions.SingleOrDefault(def => string.Equals(def.ContainerId, containerId, StringComparison.OrdinalIgnoreCase));
			return containerDef;
		}

		public ContainerDefinition? GetContainerDefinitionForType(Type t)
		{
			ContainerDefinition? containerDefForT = _containerDefinitions.SingleOrDefault(def => def.EntityType == t);
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
			var containerProperties = new ContainerProperties(id: containerDefinition.ContainerId, partitionKeyPath: containerDefinition.PartitionKeyPath);

			// add some retry logic to endure the container is created successfully. each failed attempt should wait longer than the previous one
			int[] retryIntervals = [1000, 2000, 5000, 10000, 20000];
			ContainerResponse? response = null;
			foreach (int t in retryIntervals)
			{
				try
				{
					// Create the container if it does not exist
					response = await db.CreateContainerIfNotExistsAsync(
						containerProperties: containerProperties,
						throughput: containerDefinition.Throughput);
					
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
			throw new Exception($"Failed to create container [{containerDefinition.ContainerId}] after multiple attempts");
		}
		
		public async Task<ICosmosDbContainer?> GetContainer(string containerId)
		{
			ContainerDefinition? definition = GetContainerDefinition(containerId);
			if (definition is null)
			{
				throw new ContainerDefinitionNotFoundException(containerId);
			}
			
			return await this.CreateContainer(definition);
		}

		public async Task<bool> DeleteContainer(ContainerDefinition containerDefinition)
		{
			Database database = await GetDatabase();
			Container sdkContainer = database.GetContainer(containerDefinition.ContainerId);
			ContainerResponse sdkResponse = await sdkContainer.DeleteContainerAsync();
			CosmosDbResponse<ContainerProperties> deleteResponse = sdkResponse.ToCosmosDbResponse();
			if(!deleteResponse.IsSuccessful)
			{
				// TODO: need logging
				return false;
			}

			if (!_containerCache.TryRemove(containerDefinition.EntityType, out _))
			{
				// container was not removed from cache... this should not happen
				throw new ContainerDefinitionNotDeletedException(containerDefinition);
			}

			Debug.WriteLine($"Removed Container Definition: [{containerDefinition.ContainerId}]");

			// container was removed from cache successfully
			return true;
		}

		public void Dispose()
		{
			_sdkClient.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}