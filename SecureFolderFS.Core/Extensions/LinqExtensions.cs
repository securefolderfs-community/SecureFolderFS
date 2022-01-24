using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace SecureFolderFS.Core.Extensions
{
    internal static class LinqExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || !enumerable.Any();

        public static void DisposeCollection<T>(this IEnumerable<T> enumerable)
            where T : IDisposable
        {
            foreach (var item in enumerable)
            {
                item.Dispose();
            }
        }
    }
}
