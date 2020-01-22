using AzureGems.SpendOps.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureGems.SpendOps.CosmosDB
{
	public class CosmosDbChargeTracker : IChargeTracker<CosmosDbChargedResponse>
	{
		protected readonly ILogger _logger;

		public string BuildId { get; }

		public CosmosDbChargeTracker(ILogger<CosmosDbChargeTracker> logger, IConfiguration config)
		{
			_logger = logger;

			// Get BuildId from Environment Vars
			BuildId = config["BUILD_BUILDNUMBER"];

			if (string.IsNullOrEmpty(BuildId))
			{
				BuildId = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
			}
		}

		public virtual Task Track(CosmosDbChargedResponse charge)
		{
			_logger.LogWarning("BuildId: {BuildId} Container: {ContainerId} Feature: {Feature} Context: {Context} StatusCode: {Status}, Latency: {Latency}ms, Charge: {Charge}RUs",
				BuildId,
				charge.ContainerId,
				charge.Feature,
				charge.Context != null ? string.Join(", ", charge.Context) : null,
				charge.StatusCode,
				charge.ExecutionTime.TotalMilliseconds.ToString("##.000"),
				charge.RequestCharge);

			return Task.CompletedTask;
		}
	}

}
