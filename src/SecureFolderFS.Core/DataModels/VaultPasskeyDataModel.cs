using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    public class VaultPasskeyDataModel
    {
        /// <summary>
        /// Gets the capability of the authentication method.
        /// </summary>
        /// <remarks>
        /// The capability informs how the properties of the authentication data model
        /// should be interpreted and to what capacity this attestation can be used.
        /// </remarks>
        [JsonPropertyName("capability")] // supportsVersion|supports..
        [DefaultValue("")]
        public string Capability { get; init; } = string.Empty;

        /// <summary>
        /// Gets the challenge used to authenticate the user.
        /// </summary>
        [JsonPropertyName("challenge")]
        public byte[]? Challenge { get; set; }
    }
}
