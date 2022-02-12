#nullable enable

namespace SecureFolderFS.Backend.Extensions
{
    public static class LinqExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T>? enumerable) => enumerable == null || !enumerable.Any();

        public static void DisposeCollection<T>(this IEnumerable<T> enumerable)
            where T : IDisposable
        {
            foreach (var item in enumerable)
            {
                item.Dispose();
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

        public static void EnumeratedAdd<T>(this ICollection<T> collection, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                collection.Add(item);
            }
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
