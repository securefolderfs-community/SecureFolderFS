using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Core.Instance;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels
{
    [Serializable]
    public sealed class VaultViewModel : ObservableObject
    {
        // TODO: Vault Health, Vault Properties?

        [JsonIgnore]
        public IVaultInstance? VaultInstance { get; set; }

        [JsonIgnore]
        public VaultModel VaultModel { get; set; }

        public VaultIdModel VaultIdModel { get; }

        public string VaultRootPath { get; }

        public string VaultName { get; }

        public VaultViewModel(VaultIdModel vaultIdModel, string vaultRootPath)
        {
            this.VaultIdModel = vaultIdModel;
            this.VaultRootPath = vaultRootPath;
            this.VaultName = Path.GetFileName(vaultRootPath);
            this.VaultModel = new(vaultIdModel);
        }
    }
}
