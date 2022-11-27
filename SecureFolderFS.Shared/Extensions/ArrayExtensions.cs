using System;

namespace SecureFolderFS.Shared.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Copies <paramref name="arrays"/> to <paramref name="source"/> in order.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source array to put data into.</param>
        /// <param name="arrays">The arrays.</param>
        public static void EmplaceArrays<T>(this T[] source, params T[][] arrays)
        {
            var lastOffset = 0;
            foreach (var array in arrays)
            {
                if (lastOffset == source.Length)
                    return;

                Array.Copy(array, 0, source, lastOffset, Math.Min(array.Length, source.Length - lastOffset));
                lastOffset += array.Length;
            }
        }
    }
}
