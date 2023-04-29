using System;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents an app version with platform information.
    /// </summary>
    /// <param name="Version">Gets the version of the app.</param>
    /// <param name="Platform">Gets the platform name that the app has been built to work on.</param>
    public sealed record AppVersion(Version Version, string Platform)
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Version} ({Platform})";
        }
    }
}