using DokanNet;
using SecureFolderFS.Core.FileSystem.Helpers;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan
{
    internal sealed class DokanFileSystemHelpers : IFileSystemHelpers
    {
        public bool IsNameInExpression(string expression, string name, bool ignoreCase)
        {
            return DokanHelper.DokanIsNameInExpression(expression, name, ignoreCase);
        }
    }
}
