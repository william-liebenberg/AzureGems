using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
	
namespace AzureGems.CosmosDB
{
	public class CosmosDbClientBuilder
	{
		private readonly List<ContainerDefinition> _containerDefinitions = new List<ContainerDefinition>();
		private CosmosDbConnectionSettings _connectionSettings = null;
		private CosmosDbDatabaseSettings _dbconfig = new CosmosDbDatabaseSettings(null, null);
		private ICosmosDbContainerFactory _containerFactory = null;

		public CosmosDbClientBuilder()
		{
		}

		public CosmosDbClientBuilder ReadConfiguration(IConfiguration config)
		{
			_connectionSettings = new CosmosDbConnectionSettings(config);
			_dbconfig = new CosmosDbDatabaseSettings(config);
			return this;
		}


		public CosmosDbClientBuilder WithDbConfig(CosmosDbDatabaseSettings config)
		{
			_dbconfig = config;
			return this;
		}

		public CosmosDbClientBuilder WithContainerFactory(ICosmosDbContainerFactory containerFactory)
		{
			_containerFactory = containerFactory;
			return this;
		}

		public CosmosDbClientBuilder UseDatabase(string databaseId)
		{
			_dbconfig.DatabaseId = databaseId;
			return this;
		}

		public CosmosDbClientBuilder WithSharedThroughput(int throughput)
		{
			_dbconfig.SharedThroughput = throughput;
			return this;
		}

		public CosmosDbClientBuilder Connect(string endPoint, string authKey)
		{
			_connectionSettings = new CosmosDbConnectionSettings(endPoint, authKey);
			return this;
		}


		public CosmosDbClientBuilder Connect(CosmosDbConnectionSettings connSettings)
		{
			_connectionSettings = connSettings;
			return this;
		}

		/// <summary>
		/// Set up the names, partition key paths, and optional throughput requirements for your CosmosDB containers.
		/// </summary>
		/// <param name="containerConfigBuilder">The Container Config Builder</param>
		/// <returns>The <see cref="CosmosDbClientBuilder"/></returns>
		public CosmosDbClientBuilder WithContainerConfig(Action<IContainerConfigBuilder> containerConfigBuilder)
		{
			var builder = new ContainerConfigBuilder();
			containerConfigBuilder(builder);
			_containerDefinitions.AddRange(builder.Build());
			return this;
		}

		public CosmosDbClient Build()
		{
			return new CosmosDbClient(_connectionSettings, _dbconfig, _containerFactory, _containerDefinitions);
		}
	}
}