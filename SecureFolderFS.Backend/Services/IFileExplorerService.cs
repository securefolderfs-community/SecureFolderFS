namespace SecureFolderFS.Backend.Services
{
    public interface IFileExplorerService
    {
        Task OpenPathInFileExplorerAsync(string path);
    }
}
