using DokanNet;
using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class UnmountedCallback : BaseDokanOperationsCallback, IUnmountedCallback
    {
        public UnmountedCallback(HandlesCollection handles)
            : base(handles)
        {
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            return DokanResult.Success;
        }
    }
}
