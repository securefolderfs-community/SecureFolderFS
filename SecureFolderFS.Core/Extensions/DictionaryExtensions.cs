using System.Collections.Generic;

namespace SecureFolderFS.Core.Extensions
{
    internal static class DictionaryExtensions
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
    }
}
