using Konscious.Security.Cryptography;
using System;
using System.Linq;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class Argon2idCrypt : IArgon2idCrypt
    {
        private bool _disposed;

        public byte[] Argon2idHash(byte[] password, byte[] salt)
        {
            AssertNotDisposed();

            using var argon2id = new Argon2id(password);
            argon2id.Salt = salt;
            argon2id.DegreeOfParallelism = Constants.Security.EncryptionAlgorithm.Argon2id.DEGREE_OF_PARALLELISM;
            argon2id.Iterations = Constants.Security.EncryptionAlgorithm.Argon2id.ITERATIONS;
            argon2id.MemorySize = Constants.Security.EncryptionAlgorithm.Argon2id.MEMORY_SIZE;

            return argon2id.GetBytes(32);
        }

        public bool VerifyArgon2idHash(byte[] origin, byte[] password, byte[] salt)
        {
            AssertNotDisposed();

            return origin.SequenceEqual(Argon2idHash(password, salt));
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
