using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback
{
    internal abstract class BaseDokanOperationsCallbackWithPath : BaseDokanOperationsCallback
    {
        protected readonly VaultPath vaultPath;
        protected readonly IPathConverter pathConverter;

        protected BaseDokanOperationsCallbackWithPath(VaultPath vaultPath, IPathConverter pathConverter, HandlesManager handles)
            : base(handles)
        {
            this.vaultPath = vaultPath;
            this.pathConverter = pathConverter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string GetCiphertextPath(string cleartextName)
        {
            var path = PathHelpers.ConstructFilePathFromVaultRootPath(vaultPath, cleartextName);
            return pathConverter.ToCiphertext(path);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected string GetCleartextPath(string cleartextName)
        {
            return PathHelpers.ConstructFilePathFromVaultRootPath(vaultPath, cleartextName);
        }
    }
}
