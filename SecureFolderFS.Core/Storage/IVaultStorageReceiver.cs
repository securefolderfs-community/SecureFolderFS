using SecureFolderFS.Sdk.Paths;

namespace SecureFolderFS.Core.Storage
{
    /// <summary>
    /// Provides module for retrieving instances and derivatives of <see cref="IVaultItem"/>.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IVaultStorageReceiver
    {
        IVaultFile OpenVaultFile(ICleartextPath cleartextPath);

        IVaultFile OpenVaultFile(ICiphertextPath ciphertextPath);

        IVaultFolder OpenVaultFolder(ICleartextPath cleartextPath);

        IVaultFolder OpenVaultFolder(ICiphertextPath ciphertextPath);
    }
}
