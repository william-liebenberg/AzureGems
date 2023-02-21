using Azure.Data.Tables;

namespace AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage
{
	public interface IStorageClientProvider
	{
        TableServiceClient TableClient { get; }
	}
}
