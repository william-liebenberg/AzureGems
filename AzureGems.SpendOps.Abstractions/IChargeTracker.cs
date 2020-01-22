using System.Threading.Tasks;

namespace AzureGems.SpendOps.Abstractions
{
	public interface IChargeTracker<T>
	{
		Task Track(T charge);
	}
}
