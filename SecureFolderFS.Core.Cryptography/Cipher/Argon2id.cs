using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    /// TODO: Needs docs
    public static class Argon2id
    {
        public static void DeriveKey(ReadOnlySpan<byte> password, ReadOnlySpan<byte> salt, Span<byte> result)
        {
            using var argon2id = new Konscious.Security.Cryptography.Argon2id(password.ToArray());
            argon2id.Salt = salt.ToArray();
            argon2id.DegreeOfParallelism = Constants.Crypt.CryptImpl.Argon2.DEGREE_OF_PARALLELISM;
            argon2id.Iterations = Constants.Crypt.CryptImpl.Argon2.ITERATIONS;
            argon2id.MemorySize = Constants.Crypt.CryptImpl.Argon2.MEMORY_SIZE;

            argon2id.GetBytes(Constants.ARGON2_KEK_LENGTH).CopyTo(result);
        }
    }
}
