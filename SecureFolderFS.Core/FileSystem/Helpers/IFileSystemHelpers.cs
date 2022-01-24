namespace SecureFolderFS.Core.FileSystem.Helpers
{
    internal interface IFileSystemHelpers
    {
        bool IsNameInExpression(string expression, string name, bool ignoreCase);
    }
}
