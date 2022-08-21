using Konscious.Security.Cryptography;
using System;
using System.Linq;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation
{
    internal sealed class Argon2idCrypt : IArgon2idCrypt
    {
        public byte[] Argon2idHash(byte[] password, byte[] salt)
        {
            using var argon2id = new Argon2id(password);
            argon2id.Salt = salt;
            argon2id.DegreeOfParallelism = Constants.Security.EncryptionAlgorithm.Argon2id.DEGREE_OF_PARALLELISM;
            argon2id.Iterations = Constants.Security.EncryptionAlgorithm.Argon2id.ITERATIONS;
            argon2id.MemorySize = Constants.Security.EncryptionAlgorithm.Argon2id.MEMORY_SIZE;

            return argon2id.GetBytes(32);
        }

        public bool VerifyArgon2idHash(byte[] origin, byte[] password, byte[] salt)
        {
            return origin.SequenceEqual(Argon2idHash(password, salt));
        }
    }
}
