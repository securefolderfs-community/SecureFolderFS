using System;
using System.Text.Json.Serialization;
using static SecureFolderFS.Core.Constants;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    internal class VersionDataModel
    {
        /// <summary>
        /// Gets the version of the vault.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_VERSION)]
        public required int Version { get; init; }
    }
}
