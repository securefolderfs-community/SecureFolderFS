namespace SecureFolderFS.Sdk.Storage.LockableStorage
{
    /// <summary>
    /// Represents a file that can be locked preventing file system access or deletion of it.
    /// </summary>
    public interface ILockableFile : IFile, ILockableStorable
    {
    }
}
