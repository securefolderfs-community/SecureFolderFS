using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Security.Cipher;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Helpers;

namespace SecureFolderFS.Core.VaultDataStore.VaultConfiguration
{
    [Serializable]
    public sealed class VaultConfiguration : BaseVaultConfiguration
    {
        [JsonConstructor]
        internal VaultConfiguration(int version, ContentCipherScheme contentCipherScheme, FileNameCipherScheme fileNameCipherScheme, byte[] hmacsha256Mac) 
            : base(version, contentCipherScheme, fileNameCipherScheme, hmacsha256Mac)
        {
        }

        internal static VaultConfiguration Load(RawVaultConfiguration rawVaultConfiguration)
        {
            return JsonConvert.DeserializeObject<VaultConfiguration>(rawVaultConfiguration.rawData);
        }

        internal override bool Verify(ICipherProvider keyCryptor, MasterKey masterKey)
        {
            if (HmacSha256Mac.IsEmpty() || masterKey.AnyEmpty() || keyCryptor is null)
            {
                return false;
            }

            var macKey = masterKey.GetMacKey();
            using var hmacSha256Crypt = keyCryptor.GetHmacInstance();

            hmacSha256Crypt.InitializeHmac(macKey);
            hmacSha256Crypt.Update(BitConverter.GetBytes(Version));
            hmacSha256Crypt.Update(BitConverter.GetBytes((uint)FileNameCipherScheme));
            hmacSha256Crypt.DoFinal(BitConverter.GetBytes((uint)ContentCipherScheme));

            var hmacsha256Mac = new byte[Constants.Security.EncryptionAlgorithm.HmacSha256.MAC_SIZE];
            hmacSha256Crypt.GetHash(hmacsha256Mac);

            return HmacSha256Mac.SequenceEqual(hmacsha256Mac);
        }

        internal override void WriteConfiguration(Stream destinationStream)
        {
            string serialized = JsonConvert.SerializeObject(this, Formatting.Indented);

            StreamHelpers.WriteToStream(serialized, destinationStream);
        }
    }
}
