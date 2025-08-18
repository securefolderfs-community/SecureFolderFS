using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    public record class VaultChallengeDataModel : VaultCapabilityDataModel
    {
        /// <summary>
        /// Gets the challenge used to authenticate the user.
        /// </summary>
        [JsonPropertyName("challenge")]
        [DefaultValue(null)]
        public byte[]? Challenge { get; set; }
    }
}
