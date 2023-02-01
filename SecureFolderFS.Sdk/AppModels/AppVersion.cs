using System;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed class AppVersion
    {
        public Version Version { get; }

        public string Platform { get; }

        public AppVersion(Version version, string platform)
        {
            Version = version;
            Platform = platform;
        }

        public override string ToString()
        {
            return $"{Version} ({Platform})";
        }
    }
}