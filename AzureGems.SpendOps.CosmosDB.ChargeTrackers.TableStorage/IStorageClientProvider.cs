using Microsoft.Azure.Cosmos.Table;

namespace AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage
{
	public interface IStorageClientProvider
	{
		CloudTableClient TableClient { get; }
	}
}
