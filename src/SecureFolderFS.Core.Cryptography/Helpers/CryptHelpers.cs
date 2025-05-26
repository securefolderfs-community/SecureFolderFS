using System;
using System.Runtime.CompilerServices;
using static SecureFolderFS.Core.Cryptography.Constants.CipherId;

namespace SecureFolderFS.Core.Cryptography.Helpers
{
    public static class CryptHelpers
    {
        internal static void FillAssociatedDataBe(Span<byte> associatedData, ReadOnlySpan<byte> headerNonce, long chunkNumber)
        {
            // Set first 8B of chunk number to associatedData
            Unsafe.As<byte, long>(ref associatedData[0]) = chunkNumber;

            // Reverse byte order as needed
            if (BitConverter.IsLittleEndian)
                associatedData.Slice(0, sizeof(long)).Reverse();

            // Copy header nonce after chunk number
            headerNonce.CopyTo(associatedData.Slice(sizeof(long)));
        }

        public static uint ContentCipherId(string? id)
        {
            return id switch
            {
                AES_CTR_HMAC => 1u,
                AES_GCM => 2u,
                XCHACHA20_POLY1305 => 4u,
                NONE => 256u,
                _ => 0u
            };
        }

        public static uint FileNameCipherId(string? id)
        {
            return id switch
            {
                NONE => 1u,
                AES_SIV => 2u,
                _ => 0u
            };
        }
    }
}
