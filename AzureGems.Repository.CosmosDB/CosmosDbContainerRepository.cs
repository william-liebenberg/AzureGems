﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureGems.CosmosDB;
using AzureGems.Repository.Abstractions;

namespace AzureGems.Repository.CosmosDB
{
	public class CosmosDbContainerRepository<TDomainEntity> : IRepository<TDomainEntity> where TDomainEntity : BaseEntity
	{
		private readonly IIdValueGenerator<TDomainEntity> _idValueGenerator;

		// TODO: Move to the container level and reuse it there because we have the generic type and container definition,
		// TODO: Because T in IContainer does not implement BaseEntity, set the discriminator via reflection

		private readonly IPartitionKeyResolver _pkvResolver;
		private readonly string _entityType;

		public ICosmosDbContainer Container { get; }

		public CosmosDbContainerRepository(
			ICosmosDbContainer container,
			IEntityTypeNameResolver entityTypeNameResolver,
			IIdValueGenerator<TDomainEntity> idValueGenerator,
			IPartitionKeyResolver pkvResolver
			)
		{
			Container = container;
			_idValueGenerator = idValueGenerator;
			_pkvResolver = pkvResolver;
			_entityType = entityTypeNameResolver.ResolveEntityTypeName<TDomainEntity>();
		}

		private string ResolvePartitionKeyValue(TDomainEntity entity)
		{
			return _pkvResolver.ResolvePartitionKeyValue(Container.Definition.PartitionKeyPath, entity);
		}

		private async Task<IEnumerable<TResult>> Resolve<TResult>(IQueryable<TResult> query)
		{
			CosmosDbResponse<IEnumerable<TResult>> resolvedQuery = await Container.Resolve(query);
			return resolvedQuery.Result;
		}

		public async Task<IEnumerable<TResult>> Query<TResult>(Expression<Func<IQueryable<TDomainEntity>, IQueryable<TResult>>> queryExpression)
		{
			IQueryable<TDomainEntity> query = Container.GetByLinq<TDomainEntity>();
			IQueryable<TResult> q = queryExpression.Compile().Invoke(query);
			return await Resolve(q);
		}

		public async Task<int> Count<TResult>(Expression<Func<IQueryable<TDomainEntity>, IQueryable<TResult>>> queryExpression)
		{
			IQueryable<TDomainEntity> query = Container.GetByLinq<TDomainEntity>();
			IQueryable<TResult> q = queryExpression.Compile().Invoke(query);
			CosmosDbResponse<int> response = await Container.ResolveCount(q);
			return response.Result;
		}

		public async Task<IEnumerable<TDomainEntity>> GetAll()
		{
			CosmosDbResponse<IEnumerable<TDomainEntity>> response = await Container.GetAll<TDomainEntity>();
			return response.Result;
		}

		public async Task<IEnumerable<TDomainEntity>> Get(Expression<Func<TDomainEntity, bool>> predicate)
		{
			IQueryable<TDomainEntity> query = Container.GetByLinq<TDomainEntity>()
				// add the predicate
				.Where(predicate);

			CosmosDbResponse<IEnumerable<TDomainEntity>> response = await Container.Resolve(query);
			return response.Result;
		}

		public async Task<TDomainEntity> GetById(string id)
		{
			// TODO: Passing id as pk is not the correct approach!
			CosmosDbResponse<TDomainEntity> response = await Container.Get<TDomainEntity>(id, id);
			return response.Result;
		}

		public async Task<TDomainEntity> GetById(string partitionKey, string id)
		{
			CosmosDbResponse<TDomainEntity> response = await Container.Get<TDomainEntity>(partitionKey, id);
			return response.Result;
		}

		public async Task<TDomainEntity> Add(TDomainEntity entity)
		{
			// TODO: Move the ID Value Generator and Discriminator Setter to the lower level container

			// always set new ID
			entity.Id = _idValueGenerator.Generate(entity);

			// always set the entity type / Discriminator
			entity.Discriminator = _entityType;

			CosmosDbResponse<TDomainEntity> response = await Container.Add(ResolvePartitionKeyValue(entity), entity);
			return response.Result;
		}

		public async Task<bool> Delete(string id)
		{
			TDomainEntity entity = (await Get(q => q.Id == id)).SingleOrDefault();
			if(entity == null)
			{
				return default;
			}

			return await Delete(entity);
		}

		public async Task<bool> Delete(string partitionKeyValue, string id)
		{
			CosmosDbResponse<TDomainEntity> deletedEntity = await Container.Delete<TDomainEntity>(partitionKeyValue, id);
			return deletedEntity.IsSuccessful;
		}

		public async Task<bool> Delete(TDomainEntity entity)
		{
			CosmosDbResponse<TDomainEntity> deletedEntity = await Container.Delete<TDomainEntity>(ResolvePartitionKeyValue(entity), entity.Id);
			return deletedEntity.IsSuccessful;
		}

		public async Task<TDomainEntity> Update(TDomainEntity entity)
		{
			if(string.IsNullOrWhiteSpace(entity.Id))
			{
				entity.Id = _idValueGenerator.Generate(entity);
			}

			// always set the entity type / Discriminator
			entity.Discriminator = _entityType;

			CosmosDbResponse<TDomainEntity> updatedEntity = await Container.Update(ResolvePartitionKeyValue(entity), entity);
			return updatedEntity.Result;
		}
	}
}