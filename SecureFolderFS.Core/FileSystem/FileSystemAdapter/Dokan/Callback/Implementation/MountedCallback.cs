using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class MountedCallback : BaseDokanOperationsCallback, IMountedCallback
    {
        public MountedCallback(HandlesCollection handles)
            : base(handles)
        {
        }

        public NtStatus Mounted(IDokanFileInfo info)
        {
            return DokanResult.Success;
        }
    }
}
