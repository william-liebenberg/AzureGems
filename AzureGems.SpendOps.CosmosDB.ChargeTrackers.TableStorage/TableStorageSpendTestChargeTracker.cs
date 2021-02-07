using AzureGems.CosmosDB;
using AzureGems.SpendOps.Abstractions;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage
{
	public class TableStorageSpendTestChargeTracker : ISpendTestChargeTracker<CosmosDbChargedResponse>
	{
		public interface ISettings
		{
			string RuChargeTableName { get; }
		}

		private readonly string _buildId;

		public string TestClass { get; set; }
		public string TestName { get; set; }
			   
		public TableStorageSpendTestChargeTracker(
			IStorageClientProvider tableStorageClientProvider,
			ISettings settings)
		{
			_tableStorageClientProvider = tableStorageClientProvider;
			_ruTable = new AsyncLazy<CloudTable>(async () => await CreateRuTable(settings.RuChargeTableName));

			// Get BuildId from Environment Vars
			_buildId = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");

			if (string.IsNullOrEmpty(_buildId))
			{
				_buildId = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
			}
		}

		private async Task<CloudTable> CreateRuTable(string tableName)
		{
			CloudTable rucTable = _tableStorageClientProvider.TableClient.GetTableReference(tableName);
			await rucTable.CreateIfNotExistsAsync();
			return rucTable;
		}

		private readonly IStorageClientProvider _tableStorageClientProvider;
		private readonly AsyncLazy<CloudTable> _ruTable;

		public async Task Track(CosmosDbChargedResponse charge)
		{
			DateTimeOffset now = DateTimeOffset.Now;
			string pk = (DateTimeOffset.MaxValue.Ticks - now.Ticks).ToString();
			string rk = (DateTimeOffset.MaxValue.Ticks - now.Ticks) + "." + Guid.NewGuid();

			CloudTable t = await _ruTable.Value;

			// write the new charge
			var spendEntry = new SpendTestChargeTableEntry(pk, rk, _buildId, TestClass, TestName, charge);

			TableOperation insertCharge = TableOperation.InsertOrReplace(spendEntry);
			await t.ExecuteAsync(insertCharge);
		}
	}
}
