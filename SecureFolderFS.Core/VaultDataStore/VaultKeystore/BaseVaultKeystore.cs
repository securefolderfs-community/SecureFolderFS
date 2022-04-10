using System;
using System.IO;
using Newtonsoft.Json;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Helpers;

namespace SecureFolderFS.Core.VaultDataStore.VaultKeystore
{
    [Serializable]
    internal abstract class BaseVaultKeystore
    {
        [JsonProperty("c_encryptionKey")]
        public readonly byte[] wrappedEncryptionKey;

        [JsonProperty("c_macKey")]
        public readonly byte[] wrappedMacKey;

        [JsonProperty("salt")]
        public readonly byte[] salt;

        protected BaseVaultKeystore(byte[] wrappedEncryptionKey, byte[] wrappedMacKey, byte[] salt)
        {
            this.wrappedEncryptionKey = wrappedEncryptionKey;
            this.wrappedMacKey = wrappedMacKey;
            this.salt = salt;
        }

        public virtual bool IsKeystoreValid()
        {
            return !wrappedEncryptionKey.IsEmpty()
                && !wrappedMacKey.IsEmpty()
                && !salt.IsEmpty();
        }

        public virtual void WriteKeystore(Stream destinationStream)
        {
            string serializedKeystore = JsonConvert.SerializeObject(this, Formatting.Indented);

            StreamHelpers.WriteToStream(serializedKeystore, destinationStream);
        }
    }
}
