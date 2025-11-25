using System.Diagnostics.CodeAnalysis;
using FSKit;

namespace SecureFolderFS.Core.FSKit.Callbacks
{
    /// <inheritdoc cref="FSVolume"/>
    [Experimental("APL0002")]
    internal sealed partial class MacOsVolume : FSVolume
    {
        public MacOsVolume(FSVolumeIdentifier volumeId, FSFileName volumeName)
            : base(volumeId, volumeName)
        {
        }
    }
}