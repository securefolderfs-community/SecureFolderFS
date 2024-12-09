using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Shared.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue? Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;

            return null;
        }

        public static TKV? GetByKeyOrValue<TKV>(this IDictionary<TKV, TKV> dictionary, TKV tkv)
            where TKV : class
        {
            if (dictionary.TryGetValue(tkv, out var value))
                return value;

            return dictionary.FirstOrDefault(x => x.Value == tkv).Key;
        }
    }
}
