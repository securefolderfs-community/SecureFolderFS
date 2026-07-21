using System;
using System.Collections.Generic;
using Octokit;

namespace SecureFolderFS.Uno.Platforms.Windows.ViewModels
{
    /// <inheritdoc cref="MsiInstallationViewModel"/>
    public sealed partial class WinFspInstallationViewModel() : MsiInstallationViewModel(Core.WinFsp.Constants.FileSystem.FS_ID, Core.WinFsp.Constants.FileSystem.FS_NAME)
    {
        /// <inheritdoc/>
        protected override string RepositoryOwner { get; } = "winfsp";

        /// <inheritdoc/>
        protected override string RepositoryName { get; } = "winfsp";

        /// <inheritdoc/>
        protected override string VersionTag { get; } = Core.WinFsp.Constants.FileSystem.VERSION_TAG;

        /// <inheritdoc/>
        protected override ReleaseAsset? SelectInstallerAsset(IReadOnlyList<ReleaseAsset> assets)
        {
            foreach (var item in assets)
            {
                if (item.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return null;
        }
    }
}
