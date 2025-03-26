using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Shared.Extensions
{
    public static class CollectionExtensions
    {
        public static void Insert<T>(this IList<T> collection, T item, IItemSorter<T> sorter)
        {
            var correctIndex = sorter.GetInsertIndex(item, collection);
            collection.Insert(correctIndex, item);
        }

        public static ICollection<T> ToOrAsCollection<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable is ICollection<T> collection)
                return collection;

            return enumerable.ToArray();
        }

        public static TDestination? FirstOrDefaultType<TSource, TDestination>(this IEnumerable<TSource> enumerable)
            where TDestination : class, TSource
        {
            return enumerable.FirstOrDefault(x => x is TDestination) as TDestination;
        }

        public static TDestination GetOrAdd<TSource, TDestination>(this ICollection<TSource> collection, Func<TDestination> create)
            where TDestination : class, TSource
        {
            var item = collection.FirstOrDefaultType<TSource, TDestination>();
            if (item is null)
            {
                item = create.Invoke();
                collection.Add(item);
            }

            return item;
        }

        public static void AddMultiple<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                collection.Add(item);
            }
        }

        public static T? RemoveAndGet<T>(this ICollection<T> collection, T item)
        {
            return collection.Remove(item) ? item : default;
        }

        public static T? RemoveMatch<T>(this ICollection<T> list, Func<T, bool> predicate)
        {
            foreach (var item in list)
            {
                if (predicate(item))
                    return list.Remove(item) ? item : default;
            }

            return default;
        }

        public static bool IsEmpty<T>(this IEnumerable<T>? enumerable)
        {
            if (enumerable is null)
                return true;

            if (enumerable is IList { Count: 0 })
                return true;

            return !enumerable.Any();
        }

        public static void DisposeElements<T>(this IEnumerable<T?> enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        public static void AddWithMaxCapacity<T>(this IList<T> list, T item, int maxCapacity)
        {
            if (list.Count >= maxCapacity)
                list.RemoveAt(0);

            list.Add(item);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
    }
}
