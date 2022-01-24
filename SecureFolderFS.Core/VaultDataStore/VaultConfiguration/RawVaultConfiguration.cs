using Newtonsoft.Json;
using System.IO;
using SecureFolderFS.Core.Extensions;

namespace SecureFolderFS.Core.VaultDataStore.VaultConfiguration
{
    internal sealed class RawVaultConfiguration : VaultVersion
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
            string rawData = configFileStream.ReadToEnd();

            // Get vault version
            VaultVersion vaultVersion = JsonConvert.DeserializeObject<VaultVersion>(rawData); // TODO: Use json validator

            return new RawVaultConfiguration(rawData, vaultVersion);
        }
    }
}
