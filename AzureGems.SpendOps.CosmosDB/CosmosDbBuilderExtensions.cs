using AzureGems.CosmosDB;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AzureGems.SpendOps.Abstractions;

namespace AzureGems.SpendOps.CosmosDB
{
	public static class CosmosDbBuilderExtensions
	{
		public static IServiceCollection AddSpendOps(this IServiceCollection services)
		{
			services
				.UseChargeTracker<CosmosDbChargedResponse, CosmosDbChargeTracker>()
				.TryAddSingleton<ICosmosDbContainerFactory>(provider =>
				{
					var chargeTracker = provider.GetService<IChargeTracker<CosmosDbChargedResponse>>();
					return new TrackedCosmosDbContainerFactory(chargeTracker);
				});

			return services;
		}

		public static IServiceCollection AddSpendOps<TTrackerImplementation>(this IServiceCollection services)
			where TTrackerImplementation : IChargeTracker<CosmosDbChargedResponse>
		{
			services
				.UseChargeTracker<CosmosDbChargedResponse, TTrackerImplementation>()
				.TryAddSingleton<ICosmosDbContainerFactory>(provider =>
				{
					var chargeTracker = provider.GetService<IChargeTracker<CosmosDbChargedResponse>>();
					return new TrackedCosmosDbContainerFactory(chargeTracker);
				});

			return services;
		}

		public static IServiceCollection TrackSpendTests(this IServiceCollection services)
		{
			return services.TrackSpendTests<CosmosDbChargedResponse, CosmosDbSpendTestChargeTracker>();
		}

		public static IServiceCollection TrackSpendTests<TChargeType, TTrackerImplementation>(this IServiceCollection services)
			where TTrackerImplementation : ISpendTestChargeTracker<TChargeType>
		{
			services.UseSpendTestChargeTracker<TChargeType, TTrackerImplementation>();
			return services;
		}

		private static IServiceCollection UseChargeTracker<TChargeType, TTrackerImplementation>(this IServiceCollection services)
			where TTrackerImplementation : IChargeTracker<TChargeType>
		{
			Type chargeTrackerInterface = typeof(IChargeTracker<>).MakeGenericType(typeof(TChargeType));
			Type chargeTrackerImplementation = typeof(TTrackerImplementation);
			services.AddSingleton(chargeTrackerInterface, chargeTrackerImplementation);
			return services;
		}

		private static IServiceCollection UseSpendTestChargeTracker<TChargeType, TTrackerImplementation>(this IServiceCollection services)
			where TTrackerImplementation : ISpendTestChargeTracker<TChargeType>
		{
			Type chargeTrackerInterface = typeof(IChargeTracker<>).MakeGenericType(typeof(TChargeType));
			Type spendTestChargeTrackerInterface = typeof(ISpendTestChargeTracker<>).MakeGenericType(typeof(TChargeType));
			Type spendTestChargeTrackerImplementation = typeof(TTrackerImplementation);
			services.AddSingleton(chargeTrackerInterface, spendTestChargeTrackerImplementation);
			services.AddSingleton(spendTestChargeTrackerInterface, spendTestChargeTrackerImplementation);

			return services;
		}
	}	
}
