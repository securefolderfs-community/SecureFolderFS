#nullable enable

namespace SecureFolderFS.Backend.Services
{
    public interface IFileExplorerService
    {
        Task OpenPathInFileExplorerAsync(string path);

        Task<string?> PickSingleFileAsync(IEnumerable<string>? filter);
    }
}
