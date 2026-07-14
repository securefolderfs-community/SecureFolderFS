using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    public static class Argon2id
    {
        /// <summary>
        /// Derives a KEK without blocking the calling thread.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="result">The result.</param>
        /// <remarks>
        /// Parallelism lanes: defined by <see cref="Constants.Crypto.Argon2.DEGREE_OF_PARALLELISM"/>.<br/>
        /// Iterations: defined by <see cref="Constants.Crypto.Argon2.ITERATIONS"/>.<br/>
        /// Memory: defined by <see cref="Constants.Crypto.Argon2.MEMORY_SIZE_KIBIBYTES"/>.<br/>
        /// </remarks>
        public static async Task DeriveKeyAsync(byte[] password, byte[] salt, byte[] result)
        {
            using var argon2id = new Konscious.Security.Cryptography.Argon2id(password);
            argon2id.Salt = salt;
            argon2id.DegreeOfParallelism = Constants.Crypto.Argon2.DEGREE_OF_PARALLELISM;
            argon2id.Iterations = Constants.Crypto.Argon2.ITERATIONS;
            argon2id.MemorySize = Constants.Crypto.Argon2.MEMORY_SIZE_KIBIBYTES;

            var kek = await argon2id.GetBytesAsync(Constants.KeyTraits.ARGON2_KEK_LENGTH).ConfigureAwait(false);
            kek.CopyTo(result, 0);
            CryptographicOperations.ZeroMemory(kek);
        }

        public static void V2_DeriveKey(ReadOnlySpan<byte> password, ReadOnlySpan<byte> salt, Span<byte> result)
        {
            using var argon2id = new Konscious.Security.Cryptography.Argon2id(password.ToArray());
            argon2id.Salt = salt.ToArray();
            argon2id.DegreeOfParallelism = 8;
            argon2id.Iterations = 8;
            argon2id.MemorySize = 102400;

            argon2id.GetBytes(Constants.KeyTraits.ARGON2_KEK_LENGTH).CopyTo(result);
        }

        public static void DeriveKey(ReadOnlySpan<byte> password, ReadOnlySpan<byte> salt, Span<byte> result)
        {
            using var argon2id = new Konscious.Security.Cryptography.Argon2id(password.ToArray());
            argon2id.Salt = salt.ToArray();
            argon2id.DegreeOfParallelism = Constants.Crypto.Argon2.DEGREE_OF_PARALLELISM;
            argon2id.Iterations = Constants.Crypto.Argon2.ITERATIONS;
            argon2id.MemorySize = Constants.Crypto.Argon2.MEMORY_SIZE_KIBIBYTES;

            argon2id.GetBytes(Constants.KeyTraits.ARGON2_KEK_LENGTH).CopyTo(result);
        }
    }
}
