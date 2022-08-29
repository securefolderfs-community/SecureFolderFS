using System.Collections.Generic;

namespace SecureFolderFS.Shared.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);       
            }
            else
            {
                dictionary[key] = value;
            }
        }
    }
}
