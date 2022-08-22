using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Security.EncryptionAlgorithm.CryptImplementation;

namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.Builder
{
    public sealed class EncryptionAlgorithmBuilder : IEncryptionAlgorithmBuilder
    {
        internal bool AlreadyBuilt { get; private set; }

        internal IXChaCha20Poly1305Crypt XChaCha20Poly1305Crypt { get; private set; }

        internal IAesGcmCrypt AesGcmCrypt { get; private set; }

        internal IAesCtrCrypt AesCtrCrypt { get; private set; }

        internal IAesSivCrypt AesSivCrypt { get; private set; }

        internal IArgon2idCrypt Argon2idCrypt { get; private set; }

        internal IRfc3394KeyWrap Rfc3394KeyWrap { get; private set; }

        private EncryptionAlgorithmBuilder()
        {
        }

        public static IEncryptionAlgorithmBuilder GetBuilder()
        {
            return new EncryptionAlgorithmBuilder();
        }

        public IEncryptionAlgorithmBuilder WithXChaCha20Poly1305Crypt(IXChaCha20Poly1305Crypt xChaCha20Poly1305Crypt)
        {
            AssertNotBuilt();

            XChaCha20Poly1305Crypt = xChaCha20Poly1305Crypt;
            return this;
        }

        public IEncryptionAlgorithmBuilder WithAesGcmCrypt(IAesGcmCrypt aesGcmCrypt)
        {
            AssertNotBuilt();

            AesGcmCrypt = aesGcmCrypt;
            return this;
        }

        public IEncryptionAlgorithmBuilder WithAesCtrCrypt(IAesCtrCrypt aesCtrCrypt)
        {
            AssertNotBuilt();

            AesCtrCrypt = aesCtrCrypt;
            return this;
        }

        public IEncryptionAlgorithmBuilder WithAesSivCrypt(IAesSivCrypt aesSivCrypt)
        {
            AssertNotBuilt();

            AesSivCrypt = aesSivCrypt;
            return this;
        }

        public IEncryptionAlgorithmBuilder WithArgon2idCrypt(IArgon2idCrypt argon2idCrypt)
        {
            AssertNotBuilt();

            Argon2idCrypt = argon2idCrypt;
            return this;
        }

        public IEncryptionAlgorithmBuilder WithRfc3394KeyWrap(IRfc3394KeyWrap rfc3394KeyWrap)
        {
            AssertNotBuilt();

            Rfc3394KeyWrap = rfc3394KeyWrap;
            return this;
        }

        public IEncryptionAlgorithmBuilder DoFinal()
        {
            AssertNotBuilt();
            AlreadyBuilt = true;

            // Instantiate uninitialized fields 
            XChaCha20Poly1305Crypt ??= new XChaCha20Poly1305Crypt();
            AesGcmCrypt ??= new AesGcmCrypt();
            AesCtrCrypt ??= new AesCtrCrypt();
            AesSivCrypt ??= new AesSivCrypt();
            Argon2idCrypt ??= new Argon2idCrypt();
            Rfc3394KeyWrap ??= new Rfc3394KeyWrap();

            return this;
        }

        private void AssertNotBuilt()
        {
            if (AlreadyBuilt)
            {
                throw ComponentBuilderException.AlreadyBuiltException;
            }
        }
    }
}
