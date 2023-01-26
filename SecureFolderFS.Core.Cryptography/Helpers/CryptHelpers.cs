using System;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.Cryptography.Helpers
{
    internal static class CryptHelpers
    {
        public static void FillAssociatedData(Span<byte> associatedData, ReadOnlySpan<byte> headerNonce, long chunkNumber)
        {
            // Set first 8B of chunk number to associatedData
            Unsafe.As<byte, long>(ref associatedData[0]) = chunkNumber;

            // Reverse byte order as needed
            if (BitConverter.IsLittleEndian)
                associatedData.Slice(0, sizeof(long)).Reverse();

            // Copy header nonce after chunk number
            headerNonce.CopyTo(associatedData.Slice(sizeof(long)));
        }
    }
}
