using NSec.Cryptography;
using SecureFolderFS.Core.Cryptography.Cipher;
using System;

namespace SecureFolderFS.Core.Cryptography.CryptImpl
{
    /// <inheritdoc cref="IArgon2idCrypt"/>
    public sealed class Argon2idCrypt : IArgon2idCrypt
    {
        public void DeriveKey(ReadOnlySpan<byte> password, ReadOnlySpan<byte> salt, Span<byte> result)
        {
            var argon2id = new Argon2id(new Argon2Parameters()
            {
                DegreeOfParallelism = 1, // TODO: Impl supports only 1 for now // Constants.Security.CryptImpl.Argon2.DEGREE_OF_PARALLELISM,
                MemorySize = Constants.Security.CryptImpl.Argon2.MEMORY_SIZE,
                NumberOfPasses = Constants.Security.CryptImpl.Argon2.ITERATIONS
            });

            argon2id.DeriveBytes(password, salt, result);
        }
    }
}
