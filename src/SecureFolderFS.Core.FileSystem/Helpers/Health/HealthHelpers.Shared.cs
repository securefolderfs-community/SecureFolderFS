using SecureFolderFS.Storage.Renamable;
using System;

namespace SecureFolderFS.Core.FileSystem.Helpers.Health
{
    public static partial class HealthHelpers
    {
        private static Exception FolderNotRenamable { get; } = new InvalidOperationException($"Folder is not of type {nameof(IRenamableFolder)}.");
    }
}
