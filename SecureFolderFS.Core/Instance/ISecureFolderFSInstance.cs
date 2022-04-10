using System;
using SecureFolderFS.Sdk.Paths;

namespace SecureFolderFS.Core.Instance
{
    public interface ISecureFolderFSInstance : IDisposable
    {
        string MountLocation { get; }

        IPathReceiver PathReceiver { get; }

        void StartFileSystem();

        void StopFileSystem();
    }
}
