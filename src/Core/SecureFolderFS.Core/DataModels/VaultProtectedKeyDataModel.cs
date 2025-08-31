using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    public record class VaultProtectedKeyDataModel : VaultCapabilityDataModel
    {
        /// <summary>
        /// Gets the ciphertext key material.
        /// </summary>
        [JsonPropertyName("c_protectedkey")]
        [DefaultValue(null)]
        public byte[]? CiphertextKey { get; set; }
    }
}