using Microsoft.Azure.Cosmos.Table;

namespace AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage
{
	public class ChargeTableEntry : TableEntity
	{
		public ChargeTableEntry()
		{

		}

		public ChargeTableEntry(string partitionKey, string rowKey, string buildId, CosmosDbChargedResponse charge) : base(partitionKey, rowKey)
		{
			BuildId = buildId;
			ContainerId = charge.ContainerId;
			Feature = charge.Feature;
			Context = charge.Tags != null ? string.Join(", ", charge.Tags) : null;
			StatusCode = charge.StatusCode.ToString();
			Duration = charge.ExecutionTime.TotalMilliseconds;
			Charge = charge.RequestCharge;
		}

		public string BuildId { get; set; }
		public string ContainerId { get; set; }
		public string Feature { get; set; }
		public string Context { get; set; }
		public string StatusCode { get; set; }
		public double Duration { get; set; }
		public double Charge { get; set; }
	}
}
