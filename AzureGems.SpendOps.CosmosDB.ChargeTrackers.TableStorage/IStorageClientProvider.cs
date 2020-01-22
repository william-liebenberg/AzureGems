using Microsoft.WindowsAzure.Storage.Table;

namespace AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage
{
	public interface IStorageClientProvider
	{
		CloudTableClient TableClient { get; }
	}
}
