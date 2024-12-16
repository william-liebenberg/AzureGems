using System;
using AzureGems.Repository.Abstractions;

namespace AzureGems.Repository.CosmosDB
{
	public class CosmosDbIdValueGenerator<TEntity> : IIdValueGenerator<TEntity> where TEntity : notnull, BaseEntity
	{
		public string Generate(TEntity e)
		{
			return string.IsNullOrWhiteSpace(e.Id) ? Guid.NewGuid().ToString() : e.Id;
		}
	}
}