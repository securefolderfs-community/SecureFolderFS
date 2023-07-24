using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    internal sealed class VaultAuthenticationDataModel
    {
        /// <summary>
        /// Gets the capability of the authentication method.
        /// </summary>
        /// <remarks>
        /// The capability informs how the properties of the authentication data model
        /// should be interpreted and to what capacity this attestation can be used.
        /// </remarks>
        [JsonPropertyName("capability")] // supportsVersion|supports..
        public string? Capability { get; init; }
    }
}
