using AzureGems.SpendOps.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AzureGems.SpendOps.CosmosDB
{
	public class CosmosDbSpendTestChargeTracker : CosmosDbChargeTracker, ISpendTestChargeTracker<CosmosDbChargedResponse>
	{
		public string TestClass { get; set; }
		public string TestName { get; set; }

		public CosmosDbSpendTestChargeTracker(ILogger<CosmosDbSpendTestChargeTracker> logger, IConfiguration config)
			: base(logger, config)
		{
		}

		public override Task Track(CosmosDbChargedResponse charge)
		{
			_logger.LogInformation("BuildId: {BuildId} TestClass: {TestClass} TestName: {TestName} Container: {ContainerId} Feature: {Feature} Context: {Context} StatusCode: {Status}, Latency: {Latency}ms, Charge: {Charge}RUs",
				BuildId,
				TestClass,
				TestName,
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
