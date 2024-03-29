﻿using Azure;
using Azure.Data.Tables;
using System;

namespace AzureGems.SpendOps.CosmosDB.ChargeTrackers.TableStorage
{
	public class ChargeTableEntry : ITableEntity
	{
		public ChargeTableEntry()
		{

		}

		public ChargeTableEntry(string partitionKey, string rowKey, string buildId, CosmosDbChargedResponse charge)
		{
			BuildId = buildId;
			ContainerId = charge.ContainerId;
			Feature = charge.Feature;
			Context = charge.Tags != null ? string.Join(", ", charge.Tags) : null;
			StatusCode = charge.StatusCode.ToString();
			Duration = charge.ExecutionTime.TotalMilliseconds;
			Charge = charge.RequestCharge;
			PartitionKey = partitionKey;
		}

		public string BuildId { get; set; }
		public string ContainerId { get; set; }
		public string Feature { get; set; }
		public string Context { get; set; }
		public string StatusCode { get; set; }
		public double Duration { get; set; }
		public double Charge { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
