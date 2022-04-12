using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Shared.Extensions
{
    public static class DictionaryExtensions
    {
        public static bool AddIfNotPresent<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }

        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.AddIfNotPresent(key, value))
            {
                if (dictionary.Remove(key))
                {
                    dictionary.Add(key, value);
                }
            }
        }


        public static Dictionary<TKey, TValue?> ToDictionary<TKey, TValue>(this IDictionary<TKey, TValue?> dic)
            where TKey : notnull
        {
            return Enumerable.ToDictionary(dic, kvp => kvp.Key, kvp => kvp.Value);
        }

        public static Dictionary<TKey, TValue?> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue?>> kvps)
            where TKey : notnull
        {
            return Enumerable.ToDictionary(kvps, kvp => kvp.Key, kvp => kvp.Value);
        }

        public static bool SetAndGet<TKey, TValue>(this IDictionary<TKey, TValue?> dictionary, TKey key, out TValue? value,
            Func<TValue> initializer)
        {
            if (!dictionary.TryGetValue(key, out value))
            {
                value = initializer();
                dictionary.Add(key, value);

                return true;
            }
            else if (dictionary[key] == null)
            {
                value ??= initializer();
                dictionary[key] = value;

                return true;
            }

            return false;
        }

        public static void AddOrSet<TKey, TValue>(this IDictionary<TKey, TValue?> dictionary, TKey key, TValue? value = default)
        {
            if (!dictionary.TryAdd(key, value))
            {
                dictionary[key] = value;
            }
        }
    }
}
