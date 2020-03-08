using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
	
namespace AzureGems.CosmosDB
{
	public class CosmosDbClientBuilder
	{
		public IServiceCollection Services { get; }

		private readonly List<ContainerDefinition> _containerDefinitions = new List<ContainerDefinition>();
		private CosmosDbConnectionSettings _connectionSettings = null;
		private CosmosDbDatabaseConfig _dbconfig = null;

		public CosmosDbClientBuilder(IServiceCollection services)
		{
			Services = services;
		}

		public CosmosDbClientBuilder ReadConfiguration(IConfiguration config)
		{
			_connectionSettings = new CosmosDbConnectionSettings(config);
			_dbconfig = new CosmosDbDatabaseConfig(config);
			return this;
		}

		public CosmosDbClientBuilder UseDatabase(string databaseId)
		{
			_dbconfig = _dbconfig != null ?
				_dbconfig = new CosmosDbDatabaseConfig(databaseId, _dbconfig.SharedThroughput)
				:
				_dbconfig = new CosmosDbDatabaseConfig(databaseId, null);

			return this;
		}

		public CosmosDbClientBuilder WithSharedThroughput(int throughput)
		{
			_dbconfig = _dbconfig != null ?
				new CosmosDbDatabaseConfig(_dbconfig.DatabaseId, throughput)
				:
				new CosmosDbDatabaseConfig(null, throughput);

			return this;
		}

		public CosmosDbClientBuilder UseConnection(string endPoint, string authKey)
		{
			_connectionSettings = new CosmosDbConnectionSettings(endPoint, authKey);
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
			ServiceProvider sp = Services.BuildServiceProvider();
			return new CosmosDbClient(sp, _connectionSettings, _dbconfig, _containerDefinitions);
		}
	}
}