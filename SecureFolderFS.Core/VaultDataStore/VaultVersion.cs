using System;
using Newtonsoft.Json;

namespace SecureFolderFS.Core.VaultDataStore
{
    /// <summary>
    /// Represents vault version with serialization capability.
    /// <br/>
    /// <br/>
    /// This SDK is exposed.
    /// </summary>
    [Serializable]
    public class VaultVersion
    {
        public const int HIGHEST_VERSION = 1; // Changes per vault version update

        public const int LOWEST_VERSION = V1; // May change per vault version update

        public const int V1 = 1;

        [JsonProperty("version")]
        protected internal int Version { get; }

        [JsonConstructor]
        internal VaultVersion(int version)
        {
            Version = version;
        }

        internal bool SupportsVersion(int minVersion, int maxVersion = HIGHEST_VERSION)
        {
            return 
                (Version >= minVersion && minVersion >= LOWEST_VERSION && minVersion <= HIGHEST_VERSION) && // For minVersion
                (Version <= maxVersion && maxVersion >= LOWEST_VERSION && maxVersion <= HIGHEST_VERSION);   // For maxVersion
        }

        public static bool IsVersionSupported(int version)
        {
            return version == V1; // || V2, V3, V4 ...
        }

        public static implicit operator int(VaultVersion vaultVersion) => vaultVersion.Version;
    }
}