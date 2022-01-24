using System;

namespace SecureFolderFS.Core.Extensions
{
    internal static class ArrayExtensions
    {
        /// <summary>
        /// Security Notice:
        /// <br/>
        /// This function should not be used for filling secrets.
        /// </summary>
        /// <param name="amount">The size of the array to fill the weak noise into.</param>
        /// <returns>A byte array filled with weak noise.</returns>
        public static byte[] GenerateWeakNoise(long amount)
        {
            var random = new Random();
            var noise = new byte[amount];

            random.NextBytes(noise);
            return noise;
        }

        public static T[] Slice<T>(this T[] input, int offset, int length)
            where T : new()
        {
            var output = new T[length];
            Array.Copy(input, offset, output, 0, length);

            return output;
        }

        public static byte[] AsBigEndian(this byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        public static void EmplaceArrays<T>(this T[] sourceArray, params T[][] arrays)
        {
            var lastOffset = 0;
            foreach (var array in arrays)
            {
                Array.Copy(array, 0, sourceArray, lastOffset, Math.Min(array.Length, sourceArray.Length - lastOffset));
                lastOffset += array.Length;
            }
        }

        public static T[] CloneArray<T>(this T[] array)
        {
            var clonedArray = new T[array.Length];
            Array.Copy(array, 0, clonedArray, 0, array.Length);

            return clonedArray;
        }

        public static T[] ArrayWithSize<T>(this T[] array, int size)
        {
            var newArray = new T[size];
            Array.Copy(array, 0, newArray, 0, Math.Min(array.Length, size));

            return newArray;
        }
    }
}
