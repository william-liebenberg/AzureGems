using Microsoft.WindowsAzure.Storage.Table;

namespace AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage
{
	public class SpendTestChargeTableEntry : TableEntity
	{
		public SpendTestChargeTableEntry()
		{

		}

		public SpendTestChargeTableEntry(string partitionKey, string rowKey, string buildId, string testClass, string testName, CosmosDbChargedResponse charge)
			: base(partitionKey, rowKey)
		{
			BuildId = buildId;
			TestClass = testClass;
			TestName = testName;
			ContainerId = charge.ContainerId;
			Feature = charge.Feature;
			Context = charge.Context != null ? string.Join(", ", charge.Context) : null;
			StatusCode = charge.StatusCode.ToString();
			Duration = charge.ExecutionTime.TotalMilliseconds;
			Charge = charge.RequestCharge;
		}


		public string BuildId { get; set; }

		public string TestClass { get; set; }
		public string TestName { get; set; }

		public string ContainerId { get; set; }
		public string Feature { get; set; }
		public string Context { get; set; }
		public string StatusCode { get; set; }
		public double Duration { get; set; }
		public double Charge { get; set; }
	}
}
