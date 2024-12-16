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

		private readonly ISettings _settings;

		private readonly string _buildId;

		public string TestClass { get; set; }
		public string TestName { get; set; }
			   
		public TableStorageSpendTestChargeTracker(
			IStorageClientProvider tableStorageClientProvider,
			ISettings settings)
		{
			_tableStorageClientProvider = tableStorageClientProvider;
			_settings = settings;

			// Get BuildId from Environment Vars
			_buildId = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");

			if (string.IsNullOrEmpty(_buildId))
			{
				_buildId = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
			}
		}

		private readonly IStorageClientProvider _tableStorageClientProvider;
		private TableClient _ruTable;

		private async Task<TableClient> GetRuTable()
		{
			return _ruTable ??= await CreateRuTable();
		}
		
		private async Task<TableClient> CreateRuTable()
		{
			TableClient rucTable = _tableStorageClientProvider.TableClient.GetTableClient(_settings.RuChargeTableName);
			await rucTable.CreateIfNotExistsAsync();
			return rucTable;
		}
		
		public async Task Track(CosmosDbChargedResponse charge)
		{
			TableClient table = await GetRuTable();
			
			DateTimeOffset now = DateTimeOffset.Now;
			string pk = (DateTimeOffset.MaxValue.Ticks - now.Ticks).ToString();
			string rk = (DateTimeOffset.MaxValue.Ticks - now.Ticks) + "." + Guid.NewGuid();

			// write the new charge
			var spendEntry = new SpendTestChargeTableEntry(pk, rk, _buildId, TestClass, TestName, charge);

			await table.UpsertEntityAsync(spendEntry);
		}
	}
}
