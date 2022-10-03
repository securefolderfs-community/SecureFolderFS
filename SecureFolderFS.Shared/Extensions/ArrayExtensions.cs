using System;
using System.Runtime.CompilerServices;

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
        /// Reverses the array if the architecture uses little endian.
        /// </summary>
        /// <remarks>This function does not check if <paramref name="bytes"/> is already little-endian
        /// and thus may reverse it back to big endian.</remarks>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] AsBigEndian(this byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return bytes;
        }

        /// <summary>
        /// Reverses the <paramref name="possibleLittleEndian"/> if the system uses Little Endian.
        /// </summary>
        /// <param name="possibleLittleEndian">The byte sequence that represents an integer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AsBigEndian(this Span<byte> possibleLittleEndian)
        {
            if (BitConverter.IsLittleEndian)
                possibleLittleEndian.Reverse();
        }

        /// <summary>
        /// Slices <paramref name="array"/> and copies contents onto new array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array to slice.</param>
        /// <param name="start">The start index to begin the slice at.</param>
        /// <param name="length">The count of items to slice.</param>
        /// <returns>A copy of <paramref name="array"/> with sliced data.</returns>
        public static T[] SliceArray<T>(this T[] array, int start, int length)
        {
            var arrayCopy = new T[array.Length - (start + length)];
            Array.Copy(array, start, arrayCopy, 0, length);

            return arrayCopy;
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
    }
}
