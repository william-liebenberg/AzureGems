using Azure.Data.Tables;
using AzureGems.CosmosDB;
using AzureGems.SpendOps.Abstractions;
using System;
using System.Threading.Tasks;

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
			_ruTable = new AsyncLazy<TableClient>(async () => await CreateRuTable(settings.RuChargeTableName));

			// Get BuildId from Environment Vars
			_buildId = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");

			if (string.IsNullOrEmpty(_buildId))
			{
				_buildId = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
			}
		}

		private async Task<TableClient> CreateRuTable(string tableName)
		{
			var rucTable = _tableStorageClientProvider.TableClient.GetTableClient(tableName);
			await rucTable.CreateIfNotExistsAsync();
			return rucTable;
		}

		private readonly IStorageClientProvider _tableStorageClientProvider;
		private readonly AsyncLazy<TableClient> _ruTable;

		public async Task Track(CosmosDbChargedResponse charge)
		{
			DateTimeOffset now = DateTimeOffset.Now;
			string pk = (DateTimeOffset.MaxValue.Ticks - now.Ticks).ToString();
			string rk = (DateTimeOffset.MaxValue.Ticks - now.Ticks) + "." + Guid.NewGuid();

            TableClient t = await _ruTable.Value;

			// write the new charge
			var spendEntry = new SpendTestChargeTableEntry(pk, rk, _buildId, TestClass, TestName, charge);

			await t.UpsertEntityAsync(spendEntry);
		}
	}
}
