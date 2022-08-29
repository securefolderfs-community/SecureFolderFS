namespace SecureFolderFS.Core.Security.EncryptionAlgorithm.Builder
{
    /// <summary>
    /// Provides module for injecting custom encryption algorithms and content encryption routines.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IEncryptionAlgorithmBuilder
    {
        IEncryptionAlgorithmBuilder WithXChaCha20Poly1305Crypt(IXChaCha20Poly1305Crypt xChaCha20Poly1305Crypt);

        IEncryptionAlgorithmBuilder WithAesGcmCrypt(IAesGcmCrypt aesGcmCrypt);

        IEncryptionAlgorithmBuilder WithAesCtrCrypt(IAesCtrCrypt aesCtrCrypt);

        IEncryptionAlgorithmBuilder WithArgon2idCrypt(IArgon2idCrypt argon2idCrypt);

        IEncryptionAlgorithmBuilder WithRfc3394KeyWrap(IRfc3394KeyWrap rfc3394KeyWrap);

        IEncryptionAlgorithmBuilder DoFinal();
    }
}
