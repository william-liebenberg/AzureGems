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
		private CosmosDbConnection _connection = null;
		private CosmosDbConfig _dbconfig = null;

		public CosmosDbClientBuilder(IServiceCollection services)
		{
			Services = services;
		}

		public CosmosDbClientBuilder ReadConfiguration(IConfiguration config)
		{
			_connection = new CosmosDbConnection(config);
			_dbconfig = new CosmosDbConfig(config);
			return this;
		}

		public CosmosDbClientBuilder UseDatabase(string databaseId)
		{
			_dbconfig = new CosmosDbConfig(databaseId, null);
			return this;
		}

		public CosmosDbClientBuilder WithDatabaseThroughput(int throughput)
		{
			_dbconfig = _dbconfig != null ?
				new CosmosDbConfig(_dbconfig.DatabaseId, throughput)
				:
				new CosmosDbConfig(null, throughput);

			return this;
		}

		public CosmosDbClientBuilder ConnectUsing(string endPoint, string authKey)
		{
			_connection = new CosmosDbConnection(endPoint, authKey);
			return this;
		}

		/// <summary>
		/// Set up the names, partition key paths, and optional throughput requirements for your CosmosDB containers.
		/// </summary>
		/// <param name="containerConfigBuilder">The Container Config Builder</param>
		/// <returns>The <see cref="CosmosDbClientBuilder"/></returns>
		public CosmosDbClientBuilder ContainerConfig(Action<IContainerConfigBuilder> containerConfigBuilder)
		{
			var builder = new ContainerConfigBuilder();
			containerConfigBuilder(builder);
			_containerDefinitions.AddRange(builder.Build());
			return this;
		}

		public CosmosDbClient Build()
		{
			ServiceProvider sp = Services.BuildServiceProvider();
			return new CosmosDbClient(sp, _connection, _dbconfig, _containerDefinitions);
		}
	}
}