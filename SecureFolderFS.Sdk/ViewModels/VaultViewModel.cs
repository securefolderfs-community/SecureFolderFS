using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Core.Instance;

namespace SecureFolderFS.Sdk.ViewModels
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
            VaultIdModel = vaultIdModel;
            VaultRootPath = vaultRootPath;
            VaultName = Path.GetFileName(vaultRootPath);
            VaultModel = new(vaultIdModel);
        }
    }
}
