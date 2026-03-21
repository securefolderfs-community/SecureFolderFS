using SecureFolderFS.Shared.Models;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using static SecureFolderFS.Core.Constants.Vault;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    public sealed record class V4VaultConfigurationDataModel : VersionDataModel
    {
        [JsonPropertyName(Associations.ASSOC_CONTENT_CIPHER_ID)]
        [DefaultValue("")]
        public required string ContentCipherId { get; init; }

        [JsonPropertyName(Associations.ASSOC_FILENAME_CIPHER_ID)]
        [DefaultValue("")]
        public required string FileNameCipherId { get; init; }

        [JsonPropertyName(Associations.ASSOC_FILENAME_ENCODING_ID)]
        [DefaultValue("")]
        public string FileNameEncodingId { get; set; } = Cryptography.Constants.CipherId.ENCODING_BASE64URL;

        [JsonPropertyName(Associations.ASSOC_RECYCLE_SIZE)]
        [DefaultValue(0L)]
        public long RecycleBinSize { get; set; } = 0L;

        [JsonPropertyName(Associations.ASSOC_AUTHENTICATION)]
        [DefaultValue("")]
        public required string AuthenticationMethod { get; set; } = string.Empty;

        [JsonPropertyName(Associations.ASSOC_VAULT_ID)]
        [DefaultValue("")]
        public required string Uid { get; init; } = string.Empty;

        [JsonPropertyName(Associations.ASSOC_APP_PLATFORM)]
        public AppPlatformVaultOptions? AppPlatform { get; init; }

        [JsonPropertyName("hmacsha256mac")]
        public byte[]? PayloadMac { get; set; }

        public static V4VaultConfigurationDataModel V4FromVaultOptions(VaultOptions vaultOptions)
        {
            return new()
            {
                Version = vaultOptions.Version < 1 ? Versions.LATEST_VERSION : vaultOptions.Version,
                ContentCipherId = vaultOptions.ContentCipherId ?? Cryptography.Constants.CipherId.XCHACHA20_POLY1305,
                FileNameCipherId = vaultOptions.FileNameCipherId ?? Cryptography.Constants.CipherId.AES_SIV,
                FileNameEncodingId = vaultOptions.NameEncodingId ?? Cryptography.Constants.CipherId.ENCODING_BASE64URL,
                AuthenticationMethod = vaultOptions.UnlockProcedure.ToString(),
                RecycleBinSize = vaultOptions.RecycleBinSize,
                Uid = vaultOptions.VaultId ?? Guid.NewGuid().ToString(),
                AppPlatform = vaultOptions.AppPlatform,
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes]
            };
        }

        public VaultConfigurationDataModel ToVaultConfigurationDataModel()
        {
            return new VaultConfigurationDataModel
            {
                Version = Version,
                ContentCipherId = ContentCipherId,
                FileNameCipherId = FileNameCipherId,
                FileNameEncodingId = FileNameEncodingId,
                AuthenticationMethod = AuthenticationMethod,
                RecycleBinSize = RecycleBinSize,
                Uid = Uid,
                PayloadMac = PayloadMac
            };
        }
    }
}

