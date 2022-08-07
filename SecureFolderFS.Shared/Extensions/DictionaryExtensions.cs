using System;
using System.Collections.Generic;

namespace SecureFolderFS.Shared.Extensions
{
    public static class DictionaryExtensions
    {
        public static bool AddIfNotPresent<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value());
                return true;
            }

            return false;
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> value)
        {
            if (dictionary.TryGetValue(key, out var existingValue))
            {
                return existingValue;
            }
            else
            {
                var value2 = value();
                dictionary.Add(key, value2);
                return value2;
            }
        }

        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.AddIfNotPresent(key, () => value))
            {
                if (dictionary.Remove(key))
                {
                    dictionary.Add(key, value);
                }
            }
        }
    }
}
