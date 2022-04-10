using System;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Security.KeyCrypt;
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

        internal override bool Verify(IKeyCryptor keyCryptor, MasterKey masterKey)
        {
            if (HmacSha256Mac.IsEmpty() || masterKey.IsEmpty() || keyCryptor == null)
            {
                return false;
            }

            using var macKey = masterKey.CreateMacKeyCopy();
            using var hmacSha256Crypt = keyCryptor.HmacSha256Crypt.GetInstance(macKey);

            hmacSha256Crypt.InitializeHMAC();
            hmacSha256Crypt.Update(BitConverter.GetBytes(Version));
            hmacSha256Crypt.Update(BitConverter.GetBytes((uint)FileNameCipherScheme));
            hmacSha256Crypt.DoFinal(BitConverter.GetBytes((uint)ContentCipherScheme));

            return HmacSha256Mac.SequenceEqual(hmacSha256Crypt.GetHash());
        }

        internal override void WriteConfiguration(Stream destinationStream)
        {
            string serialized = JsonConvert.SerializeObject(this, Formatting.Indented);

            StreamHelpers.WriteToStream(serialized, destinationStream);
        }
    }
}
