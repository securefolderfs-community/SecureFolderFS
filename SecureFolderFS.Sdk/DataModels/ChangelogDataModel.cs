using System;

namespace SecureFolderFS.Sdk.DataModels
{
    /// <summary>
    /// Represents an app version with platform information.
    /// </summary>
    /// <param name="Name">Gets the title of the release.</param>
    /// <param name="Description">Gets the description of changes.</param>
    /// <param name="Version">Gets the version of the app.</param>
    public sealed record ChangelogDataModel(string Name, string Description, Version Version)
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}/n{Description}";
        }
    }
}
