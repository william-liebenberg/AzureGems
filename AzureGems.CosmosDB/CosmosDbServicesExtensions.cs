using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AzureGems.CosmosDB
{
	public static class CosmosDbServicesExtensions
	{
		public static IServiceCollection AddCosmosDb(this IServiceCollection services, Action<CosmosDbBuilder> configure)
		{
			services.TryAddSingleton<ICosmosDbContainerFactory, CosmosDbContainerFactory>();

			services.AddTransient(provider =>
			{
				var config = provider.GetRequiredService<IConfiguration>();
				CosmosDbBuilder builder = new CosmosDbBuilder(services)
					.ReadConfiguration(config);

				configure(builder);

				return builder;
			});

			services.AddSingleton<ICosmosDbClient>(provider => provider.GetRequiredService<CosmosDbBuilder>().Build());

			return services;
		}
	}
}