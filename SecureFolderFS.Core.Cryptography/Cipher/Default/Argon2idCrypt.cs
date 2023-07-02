using Konscious.Security.Cryptography;
using System;

namespace SecureFolderFS.Core.Cryptography.Cipher.Default
{
    /// <inheritdoc cref="IArgon2idCrypt"/>
    public sealed class Argon2idCrypt : IArgon2idCrypt
    {
        public void DeriveKey(ReadOnlySpan<byte> password, ReadOnlySpan<byte> salt, Span<byte> result)
        {
            using var argon2id = new Argon2id(password.ToArray());
            argon2id.Salt = salt.ToArray();
            argon2id.DegreeOfParallelism = Constants.Crypt.CryptImpl.Argon2.DEGREE_OF_PARALLELISM;
            argon2id.Iterations = Constants.Crypt.CryptImpl.Argon2.ITERATIONS;
            argon2id.MemorySize = Constants.Crypt.CryptImpl.Argon2.MEMORY_SIZE;

            argon2id.GetBytes(Constants.ARGON2_KEK_LENGTH).CopyTo(result);
        }
    }
}
