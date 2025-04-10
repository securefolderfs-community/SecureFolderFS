using System;

namespace SecureFolderFS.Core.Cryptography.Cipher
{
    /// TODO: Needs docs
    public static class Argon2id
    {
        // TODO: (v3) Remove references when V3 is ready
        public static void Old_DeriveKey(ReadOnlySpan<byte> password, ReadOnlySpan<byte> salt, Span<byte> result)
        {
            using var argon2id = new Konscious.Security.Cryptography.Argon2id(password.ToArray());
            argon2id.Salt = salt.ToArray();
            argon2id.DegreeOfParallelism = 8;
            argon2id.Iterations = 8;
            argon2id.MemorySize = 102400;

            argon2id.GetBytes(Constants.KeyTraits.ARGON2_KEK_LENGTH).CopyTo(result);
        }

        public static void V3_DeriveKey(ReadOnlySpan<byte> password, ReadOnlySpan<byte> salt, Span<byte> result)
        {
            using var argon2id = new Konscious.Security.Cryptography.Argon2id(password.ToArray());
            argon2id.Salt = salt.ToArray();
            argon2id.DegreeOfParallelism = Constants.Crypto.Argon2.DEGREE_OF_PARALLELISM;
            argon2id.Iterations = Constants.Crypto.Argon2.ITERATIONS;
            argon2id.MemorySize = Constants.Crypto.Argon2.MEMORY_SIZE_KB;

            argon2id.GetBytes(Constants.KeyTraits.ARGON2_KEK_LENGTH).CopyTo(result);
        }
    }
}
