using Newtonsoft.Json;
using SecureFolderFS.Core.Helpers;
using System.IO;

namespace SecureFolderFS.Core.VaultDataStore.VaultConfiguration
{
    public sealed class RawVaultConfiguration : VaultVersion
    {
        public readonly string rawData;

        private RawVaultConfiguration(string rawData, int version)
            : base(version)
        {
            this.rawData = rawData;
        }

        public static RawVaultConfiguration Load(Stream configFileStream)
        {
            // Get data from the config file
            var rawData = StreamHelpers.ReadToEnd(configFileStream);

            // Get vault version
            var vaultVersion = JsonConvert.DeserializeObject<VaultVersion>(rawData); // TODO: Use json validator

            return new RawVaultConfiguration(rawData, vaultVersion);
        }
    }
}
