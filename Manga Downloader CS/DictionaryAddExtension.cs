using System;
using System.Collections.Generic;

static class DictionaryAddExtension
{
	public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, List<TValue>> map, TKey newKey, TValue newValue)
	{
		if(map.ContainsKey(newKey))
		{
			map[newKey].Add(newValue);
			return;
		}

		map.Add(newKey, new List<TValue>());
		map[newKey].Add(newValue);
	}
}