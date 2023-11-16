namespace SecureFolderFS.Sdk.Storage.LockableStorage
{
    /// <summary>
    /// Represents a folder that can be locked preventing file system access or deletion of it.
    /// </summary>
    public interface ILockableFolder : IFolder, ILockableStorable
    {
    }
}
