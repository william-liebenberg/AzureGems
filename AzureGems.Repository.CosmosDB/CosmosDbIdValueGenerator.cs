using System;
using AzureGems.Repository.Abstractions;

namespace AzureGems.Repository.CosmosDB
{
	public class CosmosDbIdValueGenerator<TEntity> : IIdValueGenerator<TEntity> where TEntity : BaseEntity
	{
		public string Generate(TEntity e)
		{
			return (e is null || string.IsNullOrWhiteSpace(e.Id ?? string.Empty)) ? Guid.NewGuid().ToString() : e.Id;
		}
	}
}