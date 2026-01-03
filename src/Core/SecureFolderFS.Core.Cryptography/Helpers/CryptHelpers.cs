using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.ComponentModel;
using static SecureFolderFS.Core.Cryptography.Constants.CipherId;

namespace SecureFolderFS.Core.Cryptography.Helpers
{
    public static class CryptHelpers
    {
        public static IKeyBytes GenerateChallenge(string vaultId)
        {
            var encodedVaultIdLength = Encoding.ASCII.GetByteCount(vaultId);
            var challenge = new byte[Constants.KeyTraits.CHALLENGE_KEY_PART_LENGTH + encodedVaultIdLength];

            // Fill the first CHALLENGE_KEY_PART_LENGTH bytes with secure random data
            RandomNumberGenerator.Fill(challenge.AsSpan(0, Constants.KeyTraits.CHALLENGE_KEY_PART_LENGTH));

            // Fill the remaining bytes with the ID
            // By using ASCII encoding we get 1:1 byte to char ratio which allows us
            // to use the length of the string ID as part of the SecretKey length above
            var written = Encoding.ASCII.GetBytes(vaultId, challenge.AsSpan(Constants.KeyTraits.CHALLENGE_KEY_PART_LENGTH));
            if (written != encodedVaultIdLength)
                throw new FormatException("The allocated buffer and vault ID written bytes amount were different.");

            // Return a protected key
            return ManagedKey.TakeOwnership(challenge);
        }

        internal static void FillAssociatedDataBigEndian(Span<byte> associatedData, ReadOnlySpan<byte> headerNonce, long chunkNumber)
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
