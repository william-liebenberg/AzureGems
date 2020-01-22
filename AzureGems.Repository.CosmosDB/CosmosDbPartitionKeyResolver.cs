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
			string propertyName = pathTokens.First();

			// get the value for the first path token
			Type entityType = entityInstance.GetType();
			object subEntityInstance = entityType
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase))
				.GetValue(entityInstance);

			if (pathTokens.Count() > 1)
			{
				return ResolvePathValue(pathTokens.Skip(1), subEntityInstance);
			}

			return Convert.ToString(subEntityInstance);
		}
	}
}