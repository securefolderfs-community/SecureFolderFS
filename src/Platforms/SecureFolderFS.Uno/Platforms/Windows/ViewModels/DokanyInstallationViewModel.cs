using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Octokit;

namespace SecureFolderFS.Uno.Platforms.Windows.ViewModels
{
    /// <inheritdoc cref="MsiInstallationViewModel"/>
    public sealed partial class DokanyInstallationViewModel() : MsiInstallationViewModel(Core.Dokany.Constants.FileSystem.FS_ID, Core.Dokany.Constants.FileSystem.FS_NAME)
    {
        /// <inheritdoc/>
        protected override string RepositoryOwner { get; } = "dokan-dev";

        /// <inheritdoc/>
        protected override string RepositoryName { get; } = "dokany";

        /// <inheritdoc/>
        protected override string VersionTag { get; } = Core.Dokany.Constants.FileSystem.VERSION_TAG;

        /// <inheritdoc/>
        protected override ReleaseAsset? SelectInstallerAsset(IReadOnlyList<ReleaseAsset> assets)
        {
            var archTag = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                Architecture.Arm64 => "arm64",
                _ => throw new PlatformNotSupportedException($"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
            };

            foreach (var item in assets)
            {
                var name = item.Name.ToLowerInvariant();
                if (name.EndsWith(".msi") && name.Contains(archTag))
                    return item;
            }

            return null;
        }
    }
}
