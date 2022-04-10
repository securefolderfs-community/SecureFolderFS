using System;

namespace SecureFolderFS.Shared.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Fills the array with random noise.
        /// </summary>
        /// <remarks>This function should not be used for filling secrets.</remarks>
        /// <param name="amount">The size of the array to fill the weak noise into.</param>
        /// <returns>A byte array filled with weak noise.</returns>
        public static byte[] GenerateWeakNoise(long amount)
        {
            var random = new Random();
            var noise = new byte[amount];

            random.NextBytes(noise);
            return noise;
        }

        /// <summary>
        /// Slices the <paramref name="source"/> array into regions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="offset">The offset at where to start the slice.</param>
        /// <param name="length">The length of the slice.</param>
        /// <returns></returns>
        public static T[] Slice<T>(this T[] source, int offset, int length)
            where T : new()
        {
            var output = new T[length];
            Array.Copy(source, offset, output, 0, length);

            return output;
        }

        /// <summary>
        /// Reverses the array if the architecture uses little endian.
        /// </summary>
        /// <remarks>This function does not check if <paramref name="bytes"/> is already little-endian
        /// and thus may reverse it back to big endian.</remarks>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] AsBigEndian(this byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

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
                {
                    return;
                }

                Array.Copy(array, 0, source, lastOffset, Math.Min(array.Length, source.Length - lastOffset));
                lastOffset += array.Length;
            }
        }

        /// <summary>
        /// Clones an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source array.</param>
        /// <returns></returns>
        public static T[] CloneArray<T>(this T[] source)
        {
            var clonedArray = new T[source.Length];
            Array.Copy(source, 0, clonedArray, 0, source.Length);

            return clonedArray;
        }

        /// <summary>
        /// Creates a new array with <paramref name="size"/> and copies as much contents as possible.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="size">Size of new array.</param>
        /// <returns></returns>
        public static T[] ArrayWithSize<T>(this T[] source, int size)
        {
            var newArray = new T[size];
            Array.Copy(source, 0, newArray, 0, Math.Min(source.Length, size));

            return newArray;
        }
    }
}
