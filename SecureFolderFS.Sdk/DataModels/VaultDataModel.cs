using System;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed class VaultDataModel
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public DateTime? LastAccessDate { get; set; }
    }
}
