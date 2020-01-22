using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage
{
	public class StorageClientProvider : IStorageClientProvider
	{
		public interface ISettings
		{
			string StorageAccountConnectionString { get; }
		}

		private readonly Lazy<CloudTableClient> _lazyTableClient;

		public StorageClientProvider(ISettings settings)
		{
			CloudStorageAccount sa = CloudStorageAccount.Parse(settings.StorageAccountConnectionString);

			_lazyTableClient = new Lazy<CloudTableClient>(() => CreateCloudTableClient(sa));
		}

		private static CloudTableClient CreateCloudTableClient(CloudStorageAccount sa)
		{
			return sa.CreateCloudTableClient();
		}

		public CloudTableClient TableClient => _lazyTableClient.Value;
	}
}
