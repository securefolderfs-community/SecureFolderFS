using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Core.Instance;

namespace SecureFolderFS.Sdk.ViewModels
{
    [Serializable]
    [Obsolete("This class has been deprecated. Use VaultViewModel instead.")]
    public sealed class VaultViewModelDeprecated : ObservableObject
    {
        // TODO: Vault Health, Vault Properties?

        [JsonIgnore]
        public IVaultInstance? VaultInstance { get; set; }

        [JsonIgnore]
        [Obsolete("This property should no longer be used. The type replacement is IVaultModel.")]
        public VaultModelDeprecated VaultModelDeprecated { get; set; }

        public VaultIdModel VaultIdModel { get; }

        public string VaultRootPath { get; }

        public string VaultName { get; }

        [Obsolete("This constructor has been deprecated in favor of VaultViewModel(IVaultModel).")]
        public VaultViewModelDeprecated(VaultIdModel vaultIdModel, string vaultRootPath)
        {
            VaultIdModel = vaultIdModel;
            VaultRootPath = vaultRootPath;
            VaultName = Path.GetFileName(vaultRootPath);
            VaultModelDeprecated = new(vaultIdModel);
        }
    }
}
