using System;
using Azure.Data.Tables;
using Azure.Storage.Blobs;

namespace AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage
{
	public class StorageClientProvider : IStorageClientProvider
	{
		public interface ISettings
		{
			string StorageAccountConnectionString { get; }
		}

		private readonly Lazy<TableServiceClient> _lazyTableClient;

		public StorageClientProvider(ISettings settings)
		{
            _lazyTableClient = new Lazy<TableServiceClient>(() => CreateTableServiceClient(settings.StorageAccountConnectionString));
		}

        private TableServiceClient CreateTableServiceClient(string storageAccountConnectionString)
        {
            var tc = new TableServiceClient(storageAccountConnectionString);
			return tc;
        }

        public TableServiceClient TableClient => _lazyTableClient.Value;
	}
}
