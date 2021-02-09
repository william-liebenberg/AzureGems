using AzureGems.CosmosDB;
using AzureGems.SpendOps.Abstractions;

namespace AzureGems.SpendOps.CosmosDB
{
	public class TrackedCosmosDbContainerFactory : ICosmosDbContainerFactory
	{
		private readonly IChargeTracker<CosmosDbChargedResponse> _chargeTracker;

		public TrackedCosmosDbContainerFactory(IChargeTracker<CosmosDbChargedResponse> chargeTracker)
		{
			_chargeTracker = chargeTracker;
		}

		public ICosmosDbContainer Create(ICosmosDbContainer container)
		{
			return new TrackedCosmosDbContainer(container.Definition, container, _chargeTracker, "Unspecified Feature");
		}
	}
}
