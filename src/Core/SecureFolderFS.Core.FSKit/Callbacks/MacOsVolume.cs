#pragma warning disable APL0002
using FSKit;

namespace SecureFolderFS.Core.FSKit.Callbacks
{
    /// <inheritdoc cref="FSVolume"/>
    internal sealed partial class MacOsVolume : FSVolume
    {
        public MacOsVolume(FSVolumeIdentifier volumeId, FSFileName volumeName)
            : base(volumeId, volumeName)
        {
        }
    }
}