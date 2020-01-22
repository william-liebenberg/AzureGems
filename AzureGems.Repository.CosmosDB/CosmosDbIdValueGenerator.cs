using System;
using AzureGems.Repository.Abstractions;

namespace AzureGems.Repository.CosmosDB
{
	public class CosmosDbIdValueGenerator<TEntity> : IIdValueGenerator<TEntity> where TEntity : BaseEntity
	{
		public string Generate(TEntity e)
		{
			return e != null && string.IsNullOrWhiteSpace(e.Id) ? Guid.NewGuid().ToString() : e.Id;
		}
	}
}