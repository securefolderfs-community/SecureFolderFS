using System;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed record class VaultDataModel(string? Path, string? Name, DateTime? LastAccessedDate)
    {
        public VaultDataModel()
            : this(null, null, null)
        {
        }
    }
}
