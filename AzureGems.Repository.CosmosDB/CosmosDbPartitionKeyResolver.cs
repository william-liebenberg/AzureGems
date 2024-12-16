using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AzureGems.Repository.Abstractions;

namespace AzureGems.Repository.CosmosDB
{
	public class CosmosDbPartitionKeyResolver : IPartitionKeyResolver
	{
		public string ResolvePartitionKeyValue(string partitionKeyPath, BaseEntity? entityInstance)
		{
            ArgumentNullException.ThrowIfNull(entityInstance);

            // e.g. partitionKeyPath = "/id"
            // pkPath = "/brand"
            // pkPath = "/something/somethingelse"

            string[] pathTokens = partitionKeyPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
			return ResolvePathValue(pathTokens, entityInstance);
		}

		private static string ResolvePathValue(IEnumerable<string> pathTokens, object entityInstance)
		{
			IEnumerable<string> tokens = pathTokens as string[] ?? pathTokens.ToArray();
			string propertyName = tokens.First();

			// get the value for the first path token
			Type entityType = entityInstance.GetType();
			
			object? subEntityInstance = entityType
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
				?.GetValue(entityInstance);

			if (subEntityInstance is null)
			{
				throw new Exception($"Could not resolve value of property name: [{propertyName}]");
			}

			IEnumerable<string> subTokens = tokens.Skip(1);
			return tokens.Count() > 1 ? ResolvePathValue(subTokens, subEntityInstance) : subEntityInstance.ToString() ?? string.Empty;
		}
	}
}