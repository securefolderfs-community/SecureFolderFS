using SecureFolderFS.Core.Instance;

#nullable enable

namespace SecureFolderFS.Backend.Models
{
    public sealed class UnlockedVaultModel : IDisposable
    {
        public static Dictionary<UnlockedVaultModel, IVaultInstance> Instances = new();

        public IVaultInstance? VaultInstance { get; set; }

        public VaultModel VaultModel { get; }

        public UnlockedVaultModel(VaultModel vaultModel)
        {
            this.VaultModel = vaultModel;
        }

        public void StartFileSystem()
        {
            ArgumentNullException.ThrowIfNull(VaultInstance);

            Instances.Add(this, VaultInstance);

            _ = Task.Run(() =>
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
