using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public static class AesGcm256
    {
        private const int TAG_SIZE = 16;

        public static void Encrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            if (Constants.PreferBouncyCastle)
            {
                BcEncrypt(bytes, key, nonce, tag, result, associatedData);
                return;
            }

            using var aesGcm = new AesGcm(key, TAG_SIZE);
            aesGcm.Encrypt(nonce, bytes, result, tag, associatedData);
        }

        public static void Decrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            if (Constants.PreferBouncyCastle)
            {
                BcDecrypt(bytes, key, nonce, tag, result, associatedData);
                return;
            }

            using var aesGcm = new AesGcm(key, TAG_SIZE);
            aesGcm.Decrypt(nonce, bytes, tag, result, associatedData);
        }

        public static bool TryDecrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            try
            {
                Decrypt(bytes, key, nonce, tag, result, associatedData);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        private static void BcEncrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, Span<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            var gcm = new GcmBlockCipher(new AesEngine());
            gcm.Init(true, new AeadParameters(new KeyParameter(key.ToArray()), TAG_SIZE * 8, nonce.ToArray(), associatedData.ToArray()));

            // BC concatenates ciphertext || tag into a single output buffer.
            var output = new byte[gcm.GetOutputSize(bytes.Length)];
            var written = gcm.ProcessBytes(bytes.ToArray(), 0, bytes.Length, output, 0);
            gcm.DoFinal(output, written);

            output.AsSpan(0, bytes.Length).CopyTo(result);
            output.AsSpan(bytes.Length, TAG_SIZE).CopyTo(tag);
        }

        private static void BcDecrypt(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> tag, Span<byte> result, ReadOnlySpan<byte> associatedData)
        {
            var gcm = new GcmBlockCipher(new AesEngine());
            gcm.Init(false, new AeadParameters(new KeyParameter(key.ToArray()), TAG_SIZE * 8, nonce.ToArray(), associatedData.ToArray()));

            // BC expects ciphertext || tag as one input buffer.
            var input = new byte[bytes.Length + tag.Length];
            bytes.CopyTo(input);
            tag.CopyTo(input.AsSpan(bytes.Length));

            var output = new byte[gcm.GetOutputSize(input.Length)];
            try
            {
                var written = gcm.ProcessBytes(input, 0, input.Length, output, 0);
                gcm.DoFinal(output, written);
            }
            catch (InvalidCipherTextException ex)
            {
                // Match the native AesGcm contract so TryDecrypt and callers behave identically.
                throw new CryptographicException("The authentication tag did not match.", ex);
            }

            output.AsSpan(0, result.Length).CopyTo(result);
        }
    }
}
