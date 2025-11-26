using System.Diagnostics.CodeAnalysis;
using FSKit;

namespace SecureFolderFS.Core.FSKit.Callbacks
{
    [Experimental("APL0002")]
    internal sealed class MacOsFileSystem : FSUnaryFileSystem, IFSUnaryFileSystemOperations
    {
        public void LoadResource(FSResource resource, FSTaskOptions options,
            FSUnaryFileSystemOperationsLoadResourceResult replyHandler)
        {
            var fsVolume = new FSVolume(new FSVolumeIdentifier(), FSFileName.Create("Test"));
            replyHandler(fsVolume, null);
        }

        public void UnloadResource(FSResource resource, FSTaskOptions options, FSUnaryFileSystemOperationsUnloadResourceResult reply)
        {
            reply(null);
        }
    }
}
