using System;

namespace AzureGems.CosmosDB
{
	public interface ICosmosDbContainerFactory
	{
		/// <summary>
		/// Create customised CosmosDbContainers
		/// </summary>
		/// <param name="container">The base-level container obtained directly from the CosmosDb Database</param>
		/// <returns></returns>
		ICosmosDbContainer Create(ICosmosDbContainer container);
	}
}