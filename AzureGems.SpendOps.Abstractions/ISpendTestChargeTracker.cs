namespace AzureGems.SpendOps.Abstractions
{
	public interface ISpendTestChargeTracker
	{
		string TestClass { get; set; }
		string TestName { get; set; }
	}

	public interface ISpendTestChargeTracker<T> : ISpendTestChargeTracker, IChargeTracker<T>
	{
	}
}
