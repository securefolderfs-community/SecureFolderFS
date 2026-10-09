using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using static SecureFolderFS.Core.Constants.Vault;

namespace SecureFolderFS.Core.DataModels
{
    /// <summary>
    /// Represents the subset of the vault configuration that describes how a vault is unlocked.
    /// </summary>
    /// <remarks>
    /// These members are shared by every configuration format since V2. Reading only them allows the login
    /// sequence to be assembled for outdated vaults awaiting migration, whose configuration cannot be
    /// deserialized into <see cref="VaultConfigurationDataModel"/> because it lacks members introduced later.
    /// </remarks>
    [Serializable]
    public sealed record class VaultAuthenticationDataModel : VersionDataModel
    {
        /// <summary>
        /// Gets the information about the authentication method used for this vault.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_AUTHENTICATION)]
        [DefaultValue("")]
        public string AuthenticationMethod { get; init; } = string.Empty;

        /// <summary>
        /// Gets the unique identifier of the vault represented by a GUID.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_VAULT_ID)]
        [DefaultValue("")]
        public string Uid { get; init; } = string.Empty;
    }
}
