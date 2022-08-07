using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed class VaultContextDataModel
    {
        public DateTime LastAccessedDate { get; set; }

        [JsonConstructor]
        public VaultContextDataModel(DateTime lastAccessedDate = default)
        {
            LastAccessedDate = lastAccessedDate;
        }
    }
}
