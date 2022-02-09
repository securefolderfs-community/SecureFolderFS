using SecureFolderFS.Backend.Extensions;
using SecureFolderFS.Core.Instance;

#nullable enable

namespace SecureFolderFS.Backend.Models
{
    public sealed class UnlockedVaultModel : IDisposable
    {
        public IVaultInstance? VaultInstance { get; set; }

        public VaultModel VaultModel { get; }

        public UnlockedVaultModel(VaultModel vaultModel)
        {
            this.VaultModel = vaultModel;
        }

        public void StartFileSystem()
        {
            ArgumentNullException.ThrowIfNull(VaultInstance);

            AsyncExtensions.RunAndForget(() =>
            {
                VaultInstance.SecureFolderFSInstance.StartFileSystem();
            });
        }

        public void Dispose()
        {
            VaultInstance?.Dispose();
        }
    }
}
