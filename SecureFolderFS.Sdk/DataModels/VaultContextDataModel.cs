using System;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed class VaultContextDataModel
    {
        public DateTime LastAccessedDate { get; set; }

        public VaultContextDataModel(DateTime lastAccessedDate)
        {
            LastAccessedDate = lastAccessedDate;
        }
    }
}
