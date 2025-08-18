using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    public record class VaultChallengeComplementation : VaultChallengeDataModel
    {
        /// <summary>
        /// Gets the complementation copy of the keystore that the authentication method unlocks.
        /// </summary>
        [JsonPropertyName("complementation")]
        [DefaultValue(null)]
        private VaultKeystoreDataModel? ComplementedKeystore { get; set; }
    }
}
