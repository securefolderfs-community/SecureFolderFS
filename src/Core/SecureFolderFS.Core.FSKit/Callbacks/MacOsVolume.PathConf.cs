using System;
using FSKit;

namespace SecureFolderFS.Core.FSKit.Callbacks
{
    internal sealed partial class MacOsVolume : IFSVolumePathConfOperations
    {
        /// <inheritdoc/>
        public IntPtr MaximumLinkCount { get; } = -1;

        /// <inheritdoc/>
        public IntPtr MaximumNameLength { get; } = -1;

        /// <inheritdoc/>
        public bool RestrictsOwnershipChanges { get; } = false;

        /// <inheritdoc/>
        public bool TruncatesLongNames { get; } = false;

        /// <inheritdoc/>
        public IntPtr MaximumXattrSize { get; } = int.MaxValue;

        /// <inheritdoc/>
        public ulong MaximumFileSize { get; } = ulong.MaxValue;
    }
}