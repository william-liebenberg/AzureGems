using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AzureGems.CosmosDB
{
	public static class ConcurrentDictionaryExtension
	{
		public static async Task<TValue> GetOrAddAsync<TKey, TValue>(
			this ConcurrentDictionary<TKey, TValue> dictionary,
			TKey key, Func<TKey, Task<TValue>> valueFactory) where TKey : notnull
		{
			if (dictionary.TryGetValue(key, out TValue? resultingValue))
			{
				return resultingValue;
			}

			TValue value = await valueFactory(key);
			
			return dictionary.GetOrAdd(key, value);
		}
	}
}