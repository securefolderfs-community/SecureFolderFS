using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    /// <summary>
    /// Defines the basic operations of cryptographic transformations using <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public interface ICryptoTransformSpan : ICryptoTransform
    {
        /// <summary>
        /// Transforms the specified region of the input <see cref="ReadOnlySpan{T}"/> and copies the resulting transform to the output <see cref="Span{T}"/>.
        /// </summary>
        /// <remarks>
        /// The return value of <see cref="TransformBlock"/> is the number of bytes returned to <paramref name="outputBuffer"/> and is always less than or equal
        /// to <see cref="ICryptoTransform.OutputBlockSize"/>. If <see cref="ICryptoTransform.CanTransformMultipleBlocks"/> is true, then the size of <paramref name="inputBuffer"/>
        /// must be any positive multiple of <see cref="ICryptoTransform.InputBlockSize"/>.
        /// </remarks>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="outputBuffer">The output to which to write the transform.</param>
        /// <returns>The number of bytes written.</returns>
        int TransformBlock(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer);

        /// <summary>
        /// Transforms the specified region of the specified byte array.
        /// </summary>
        /// <remarks>
        /// <see cref="TransformFinalBlock"/> is a special function for transforming the last block or a partial block in the stream.
        /// </remarks>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="outputBuffer">The output to which to write the computed transform.</param>
        /// <returns>The number of bytes written.</returns>
        int TransformFinalBlock(ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer);

        /// <summary>
        /// Calculates the transform length for the output of <see cref="TransformFinalBlock"/>.
        /// </summary>
        /// <param name="inputLength">The number of bytes used for input for which the transform length is calculated.</param>
        /// <returns>The number of bytes used for the final transform.</returns>
        int CalculateFinalTransform(int inputLength);
    }
}
