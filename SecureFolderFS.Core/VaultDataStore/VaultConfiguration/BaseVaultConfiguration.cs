using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Security.Cipher;

namespace SecureFolderFS.Core.VaultDataStore.VaultConfiguration
{
    [Serializable]
    public abstract class BaseVaultConfiguration : VaultVersion
    {
        private const uint UNDEFINED_CONTENT_ENCRYPTION_SCHEME = (uint)ContentCipherScheme.Undefined; // Type safety for Undefined

        private const uint UNDEFINED_FILENAME_ENCRYPTION_SCHEME = (uint)FileNameCipherScheme.Undefined; // Type safety for Undefined

        /// <summary>
        /// The cipher configuration for file content encryption.
        /// </summary>
        [JsonProperty("contentCipherScheme", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(UNDEFINED_CONTENT_ENCRYPTION_SCHEME)]
        public ContentCipherScheme ContentCipherScheme { get; }

        /// <summary>
        /// The cipher configuration for filename encryption.
        /// </summary>
        [JsonProperty("filenameCipherScheme", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(UNDEFINED_FILENAME_ENCRYPTION_SCHEME)]
        public FileNameCipherScheme FileNameCipherScheme { get; }

        /// <summary>
        /// The HMAC-SHA256 hash of payload.
        /// </summary>
        [JsonProperty("hmacsha256mac")]
        public byte[] HmacSha256Mac { get; }

        protected internal BaseVaultConfiguration(int version, ContentCipherScheme contentCipherScheme, FileNameCipherScheme fileNameCipherScheme, byte[] hmacSha256Mac)
            : base(version)
        {
            ContentCipherScheme = contentCipherScheme;
            FileNameCipherScheme = fileNameCipherScheme;
            HmacSha256Mac = hmacSha256Mac;
        }

        internal abstract void WriteConfiguration(Stream destinationStream);

        internal abstract bool Verify(ICipherProvider keyCryptor, MasterKey masterKey);
    }
}
