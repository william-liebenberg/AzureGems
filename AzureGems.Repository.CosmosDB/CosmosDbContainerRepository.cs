using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AzureGems.CosmosDB;
using AzureGems.Repository.Abstractions;
using Microsoft.Azure.Cosmos.Linq;

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
			return resolvedQuery.Result ?? [];
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

        public async Task<IEnumerable<TResult>> Query<TResult>(string partitionKey, Expression<Func<IQueryable<TDomainEntity>, IQueryable<TResult>>> queryExpression)
        {
            IQueryable<TDomainEntity> query = Container.GetByLinq<TDomainEntity>(partitionKey);
            IQueryable<TResult> q = queryExpression.Compile().Invoke(query);
            return await Resolve(q);
        }
        
        public async Task<IEnumerable<TDomainEntity>> GetAll()
		{
			CosmosDbResponse<IEnumerable<TDomainEntity>> response = await Container.GetAll<TDomainEntity>();
			return response.Result ?? [];
		}

        public async Task<IEnumerable<TDomainEntity>> GetAll(string partitionKey)
        {
            CosmosDbResponse<IEnumerable<TDomainEntity>> response =
                await Container.GetByQuery<TDomainEntity>(partitionKey, "SELECT * FROM c");
            return response.Result ?? [];
        }

        public async Task<IEnumerable<TDomainEntity>> Get(Expression<Func<TDomainEntity, bool>> predicate)
		{
			IQueryable<TDomainEntity> query = Container.GetByLinq<TDomainEntity>()
				.Where(predicate);

			CosmosDbResponse<IEnumerable<TDomainEntity>> response = await Container.Resolve(query);
			return response.Result ?? [];
		}

        public async Task<IEnumerable<TDomainEntity>> Get(string partitionKey, Expression<Func<TDomainEntity, bool>> predicate)
        {
            IQueryable<TDomainEntity> query = Container.GetByLinq<TDomainEntity>()
                // add the predicate
                .Where(predicate);

            CosmosDbResponse<IEnumerable<TDomainEntity>> response =
                await Container.GetByQuery<TDomainEntity>(partitionKey, query.ToQueryDefinition().QueryText);
            return response.Result ?? [];
        }

        public async Task<IEnumerable<TDomainEntity>> GetByQuery(string partitionKey, string query)
        {
            CosmosDbResponse<IEnumerable<TDomainEntity>> response = await Container.GetByQuery<TDomainEntity>(partitionKey, query);
            return response.Result ?? [];
        }

        public async Task<TDomainEntity?> GetById(string id)
		{
			// TODO: Passing id as pk is not the correct approach!
			CosmosDbResponse<TDomainEntity> response = await Container.Get<TDomainEntity>(id, id);
			if (response.Result is null)
			{
				return default;
			}
			return response.Result;
		}

		public async Task<TDomainEntity?> GetById(string partitionKey, string id)
		{
			CosmosDbResponse<TDomainEntity> response = await Container.Get<TDomainEntity>(partitionKey, id);
			if (response.Result is null)
			{
				return default;
			}
			return response.Result;
		}

		public async Task<TDomainEntity> Add(TDomainEntity entity)
		{
			// TODO: Move the ID Value Generator and Discriminator Setter to the lower level container

			// always set new ID
			entity.Id = _idValueGenerator.Generate(entity);

			// always set the entity type / Discriminator
			entity.Discriminator = _entityType;

			// resolve the PK value for this entity in this specific container
			string partitionKey = ResolvePartitionKeyValue(entity);
			
			CosmosDbResponse<TDomainEntity> response = await Container.Add(partitionKey, entity);
			if (response.Result is null)
			{
				throw new Exception($"Could not add entity with pk/id: [{partitionKey}/{entity.Id}]");
			}
			return response.Result;
		}

		public async Task<bool> Delete(string id)
		{
			TDomainEntity? entity = (await Get(q => q.Id == id)).SingleOrDefault();
			if (entity is null)
			{
				return false;
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
			if(string.IsNullOrWhiteSpace(entity.Id))
			{
				throw new ArgumentException("Cannot delete entity without an ID");
			}
			
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

			// resolve the PK value for this entity in this specific container
			string partitionKey = ResolvePartitionKeyValue(entity);

			CosmosDbResponse<TDomainEntity> updatedEntity = await Container.Update(partitionKey, entity);
			if (updatedEntity.Result is null)
			{
				throw new Exception($"Could not update entity with pk/id: [{partitionKey}/{entity.Id}]");
			}
			return updatedEntity.Result;
		}

        public async Task<IEnumerable<TResult>> ExecuteQuery<TResult>(string partitionKey, string query)
        {
            CosmosDbResponse<IEnumerable<TResult>> response = await Container.GetByQuery<TResult>(partitionKey, query);
            return response.Result ?? [];
        }

        public async Task<IEnumerable<TResult>> ExecuteQuery<TResult>(string query)
        {
            CosmosDbResponse<IEnumerable<TResult>> response = await Container.GetByQuery<TResult>(query);
            return response.Result ?? [];
        }
    }
}