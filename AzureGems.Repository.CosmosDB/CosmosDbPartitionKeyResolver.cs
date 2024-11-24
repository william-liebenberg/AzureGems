using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AzureGems.Repository.CosmosDB
{
	public class CosmosDbPartitionKeyResolver : IPartitionKeyResolver
	{
		public string ResolvePartitionKeyValue(string partitionKeyPath, object entityInstance)
		{
			// e.g. partitionKeyPath = "/id"
			// pkPath = "/brand"
			// pkPath = "/something/somethingelse"

			string[] pathTokens = partitionKeyPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			return ResolvePathValue(pathTokens, entityInstance);
		}

		private string ResolvePathValue(IEnumerable<string> pathTokens, object entityInstance)
		{
			IEnumerable<string> tokens = pathTokens as string[] ?? pathTokens.ToArray();
			string propertyName = tokens.First();

			// get the value for the first path token
			Type entityType = entityInstance.GetType();
			
			object subEntityInstance = entityType
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
				?.GetValue(entityInstance);

			return tokens.Count() > 1 ? ResolvePathValue(tokens.Skip(1), subEntityInstance) 
				: Convert.ToString(subEntityInstance);
		}
	}
}