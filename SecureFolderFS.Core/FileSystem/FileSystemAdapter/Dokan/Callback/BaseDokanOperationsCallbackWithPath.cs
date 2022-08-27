using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Sdk.Paths;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback
{
    internal abstract class BaseDokanOperationsCallbackWithPath : BaseDokanOperationsCallback
    {
        protected readonly VaultPath vaultPath;
        protected readonly IPathReceiver pathReceiver;

        protected BaseDokanOperationsCallbackWithPath(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesManager handles)
            : base(handles)
        {
            this.vaultPath = vaultPath;
            this.pathReceiver = pathReceiver;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string GetCiphertextPath(string cleartextName)
        {
            var path = PathHelpers.ConstructFilePathFromVaultRootPath(vaultPath, cleartextName);
            return pathReceiver.ToCiphertext(path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string GetCleartextPath(string cleartextName)
        {
            return PathHelpers.ConstructFilePathFromVaultRootPath(vaultPath, cleartextName);
        }
    }
}
