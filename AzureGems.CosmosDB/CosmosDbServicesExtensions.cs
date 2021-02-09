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
			services.AddTransient(provider =>
			{
				var config = provider.GetRequiredService<IConfiguration>();
				var containerFactory = provider.GetService<ICosmosDbContainerFactory>();

				CosmosDbClientBuilder builder = new CosmosDbClientBuilder()
					.ReadConfiguration(config);

				builder.WithContainerFactory(containerFactory);

				configure?.Invoke(builder);

				return builder;
			});

			services.AddSingleton<ICosmosDbClient>(provider => provider.GetRequiredService<CosmosDbClientBuilder>().Build());

			return services;
		}
	}
}