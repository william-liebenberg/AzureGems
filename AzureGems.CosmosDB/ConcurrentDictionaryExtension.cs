using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AzureGems.CosmosDB
{
	public static class ConcurrentDictionaryExtension
	{
		public static async Task<TValue> GetOrAddAsync<TKey, TValue>(
			this ConcurrentDictionary<TKey, TValue> dictionary,
			TKey key, Func<TKey, Task<TValue>> valueFactory)
		{
			return dictionary.TryGetValue(key, out TValue resultingValue) ? resultingValue : dictionary.GetOrAdd(key, await valueFactory(key));
		}
	}
}