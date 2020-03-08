using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AzureGems.CosmosDB
{
	public static class CosmosDbServicesExtensions
	{
		public static IServiceCollection AddCosmosDb(this IServiceCollection services, Action<CosmosDbClientBuilder> configure = null)
		{
			services.TryAddSingleton<ICosmosDbContainerFactory, CosmosDbContainerFactory>();

			services.AddTransient(provider =>
			{
				var config = provider.GetRequiredService<IConfiguration>();
				CosmosDbClientBuilder builder = new CosmosDbClientBuilder(services)
					.ReadConfiguration(config);

				configure?.Invoke(builder);

				return builder;
			});

			services.AddSingleton<ICosmosDbClient>(provider => provider.GetRequiredService<CosmosDbClientBuilder>().Build());

			return services;
		}
	}
}