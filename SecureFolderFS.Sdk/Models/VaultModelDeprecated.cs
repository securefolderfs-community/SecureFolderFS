using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace SecureFolderFS.Sdk.Models
{
    [Serializable]
    [Obsolete("This class has been deprecated. Use IVaultModel instead.")]
    public sealed class VaultModelDeprecated : ObservableObject
    {
        [JsonIgnore]
        public VaultIdModel VaultIdModel { get; }

        private DateTime _LastOpened;
        public DateTime LastOpened
        {
            get => _LastOpened;
            set => SetProperty(ref _LastOpened, value);
        }

        private DateTime _LastScanned;
        public DateTime LastScanned
        {
            get => _LastOpened;
            set => SetProperty(ref _LastScanned, value);
        }

        public VaultModelDeprecated(VaultIdModel vaultIdModel)
        {
            VaultIdModel = vaultIdModel;
        }
    }
}
