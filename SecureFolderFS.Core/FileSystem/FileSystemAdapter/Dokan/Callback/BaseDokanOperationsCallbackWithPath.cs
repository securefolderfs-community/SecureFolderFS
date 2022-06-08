using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Sdk.Paths;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback
{
    internal abstract class BaseDokanOperationsCallbackWithPath : BaseDokanOperationsCallback
    {
        protected readonly VaultPath vaultPath;

        protected readonly IPathReceiver pathReceiver;

        protected BaseDokanOperationsCallbackWithPath(VaultPath vaultPath, IPathReceiver pathReceiver, HandlesCollection handles)
            : base(handles)
        {
            vaultPath = vaultPath;
            pathReceiver = pathReceiver;
        }

        protected void ConstructFilePath(string fileName, out ICleartextPath cleartextPath)
        {
            var filePath = PathHelpers.ConstructFilePathFromVaultRootPath(vaultPath, fileName);

            cleartextPath = pathReceiver.FromCleartextPath<ICleartextPath>(filePath);
        }

        protected void ConstructFilePath(string fileName, out ICiphertextPath ciphertextPath)
        {
            var filePath = PathHelpers.ConstructFilePathFromVaultRootPath(vaultPath, fileName);

            ciphertextPath = pathReceiver.FromCleartextPath<ICiphertextPath>(filePath);
        }

        protected void ConstructFilePath(string fileName, out ICleartextPath cleartextPath, out ICiphertextPath ciphertextPath)
        {
            var filePath = PathHelpers.ConstructFilePathFromVaultRootPath(vaultPath, fileName);

            cleartextPath = pathReceiver.FromCleartextPath<ICleartextPath>(filePath);
            ciphertextPath = pathReceiver.FromCleartextPath<ICiphertextPath>(filePath);
        }
    }
}
