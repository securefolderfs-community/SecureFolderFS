using Newtonsoft.Json;
using System;
using System.IO;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Extensions;

namespace SecureFolderFS.Core.VaultDataStore.VaultKeystore
{
    [Serializable]
    internal sealed class VaultKeystore : BaseVaultKeystore
    {
        public VaultKeystore(byte[] wrappedEncryptionKey, byte[] wrappedMacKey, byte[] salt)
            : base(wrappedEncryptionKey, wrappedMacKey, salt)
        {
        }

        public static VaultKeystore Load(Stream keystoreFileStream)
        {
            // Get data from keystore file
            string rawData = keystoreFileStream.ReadToEnd();

            // Get keystore instance
            VaultKeystore vaultKeystore = JsonConvert.DeserializeObject<VaultKeystore>(rawData);

            return vaultKeystore;
        }
    }
}
