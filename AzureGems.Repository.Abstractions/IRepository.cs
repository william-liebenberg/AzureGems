using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AzureGems.Repository.Abstractions
{
	public interface IRepository<TEntity> where TEntity : BaseEntity
	{
		Task<IEnumerable<TEntity>> GetAll();
		Task<TEntity> GetById(string id);
		Task<TEntity> GetById(string partitionKey, string id);
		Task<IEnumerable<TEntity>> Get(Expression<Func<TEntity, bool>> predicate);
		Task<TEntity> Add(TEntity entity);
		Task<TEntity> Update(TEntity entity);
		Task<bool> Delete(string id);
		Task<bool> Delete(string partitionKey, string id);
		Task<bool> Delete(TEntity entity);
		Task<IEnumerable<TResult>> Query<TResult>(Expression<Func<IQueryable<TEntity>, IQueryable<TResult>>> queryExpression);
		Task<int> Count<TResult>(Expression<Func<IQueryable<TEntity>, IQueryable<TResult>>> queryExpression);
	}
}
