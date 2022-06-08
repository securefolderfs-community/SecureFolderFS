using Newtonsoft.Json;
using System.IO;
using SecureFolderFS.Core.Extensions;

namespace SecureFolderFS.Core.VaultDataStore.VaultConfiguration
{
    public sealed class RawVaultConfiguration : VaultVersion
    {
        public readonly string rawData;

        private RawVaultConfiguration(string rawData, int version)
            : base(version)
        {
            rawData = rawData;
        }

        public static RawVaultConfiguration Load(Stream configFileStream)
        {
            // Get data from the config file
            var rawData = configFileStream.ReadToEnd();

            // Get vault version
            var vaultVersion = JsonConvert.DeserializeObject<VaultVersion>(rawData); // TODO: Use json validator

            return new RawVaultConfiguration(rawData, vaultVersion);
        }
    }
}
