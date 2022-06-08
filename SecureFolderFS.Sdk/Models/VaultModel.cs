using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace SecureFolderFS.Sdk.Models
{
    [Serializable]
    public sealed class VaultModel : ObservableObject
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

        public VaultModel(VaultIdModel vaultIdModel)
        {
            VaultIdModel = vaultIdModel;
        }
    }
}
