using AzureGems.CosmosDB;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AzureGems.SpendOps.Abstractions;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class CosmosDbBuilderExtensions
	{
		public static IServiceCollection UseDefaultChargeTracker(this IServiceCollection services)
		{
			return services.UseChargeTracker<CosmosDbChargedResponse, CosmosDbChargeTracker>();
		}

		public static IServiceCollection UseDefaultSpendTestTracker(this IServiceCollection services)
		{
			return services.UseChargeTracker<CosmosDbChargedResponse, CosmosDbSpendTestChargeTracker>();
		}

		/// <summary>
		/// Add your custom <see cref="IChargeTracker"/>
		/// </summary>
		/// <typeparam name="TChargeType">The charge type (e.g. <see cref="CosmosDbChargedResponse"/>)</typeparam>
		/// <typeparam name="TTrackerImplementation"></typeparam>
		/// <returns>The <see cref="CosmosDbClientBuilder"/></returns>
		public static IServiceCollection UseChargeTracker<TChargeType, TTrackerImplementation>(this IServiceCollection services)
			where TTrackerImplementation : IChargeTracker<TChargeType>
		{
			Type chargeTrackerInterface = typeof(IChargeTracker<>).MakeGenericType(typeof(TChargeType));
			Type chargeTrackerImplementation = typeof(TTrackerImplementation);
			services.AddSingleton(chargeTrackerInterface, chargeTrackerImplementation);
			return services;
		}

		/// <summary>
		/// Add your custom <see cref="ISpendTestChargeTracker"/>
		/// </summary>
		/// <typeparam name="TChargeType">The charge type (e.g. <see cref="CosmosDbChargedResponse"/>)</typeparam>
		/// <typeparam name="TTrackerImplementation"></typeparam>
		/// <returns>The <see cref="CosmosDbClientBuilder"/></returns>
		public static IServiceCollection UseSpendTestChargeTracker<TChargeType, TTrackerImplementation>(this IServiceCollection services)
			where TTrackerImplementation : ISpendTestChargeTracker<TChargeType>
		{
			Type chargeTrackerInterface = typeof(IChargeTracker<>).MakeGenericType(typeof(TChargeType));
			Type spendTestChargeTrackerInterface = typeof(ISpendTestChargeTracker<>).MakeGenericType(typeof(TChargeType));
			Type spendTestChargeTrackerImplementation = typeof(TTrackerImplementation);
			services.AddSingleton(chargeTrackerInterface, spendTestChargeTrackerImplementation);
			services.AddSingleton(spendTestChargeTrackerInterface, spendTestChargeTrackerImplementation);

			return services;
		}

		public static IServiceCollection AddSpendOps(this IServiceCollection services)
		{
			services.TryAddSingleton<ICosmosDbContainerFactory>(provider =>
			{
				var factory = new CosmosDbContainerFactory
				{
					Provider = (creatorType, containerDefinition, cosmosDbClient) =>
					{
						var chargeTracker = cosmosDbClient.ServiceProvider.GetService<IChargeTracker<CosmosDbChargedResponse>>();
						ICosmosDbContainer container = cosmosDbClient.GetContainer(containerDefinition.ContainerId)
							.ConfigureAwait(false).GetAwaiter().GetResult();
						return new TrackedCosmosDbContainer(containerDefinition, container, chargeTracker, creatorType.Name);
					}
				};

				return factory;
			});

			return services;
		}
	}	
}
