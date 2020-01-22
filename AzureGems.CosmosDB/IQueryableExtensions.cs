using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AzureGems.CosmosDB
{
	public static class IQueryableExtensions
	{
		private const string DiscriminatorPropertyName = "Discriminator";

		public static IQueryable<T> WithDiscriminator<T>(this IQueryable<T> source, string discriminator)
		{
			Type type = typeof(T);
			PropertyInfo discriminatorProperty = type.GetProperty(DiscriminatorPropertyName);
			if(discriminatorProperty == null)
			{
				return source;
			}

			ParameterExpression arg = Expression.Parameter(type, "p");
			MemberExpression prop = Expression.Property(arg, DiscriminatorPropertyName);
			ConstantExpression discriminatorConstant = Expression.Constant(discriminator);
			BinaryExpression body = Expression.Equal(prop, discriminatorConstant);
			Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(body, arg);

			return source.Where(predicate);
		}

		public static IQueryable<T> Page<T>(this IQueryable<T> query, int page, int pageSize = 10)
		{
			return query.Skip(page * pageSize).Take(pageSize);
		}	
	}
}