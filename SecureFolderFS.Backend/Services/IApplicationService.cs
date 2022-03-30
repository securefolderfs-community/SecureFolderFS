namespace SecureFolderFS.Backend.Services
{
    public interface IApplicationService
    {
        Version GetAppVersion();

        void CloseApplication();

        Task OpenUriAsync(Uri uri);

        Task OpenAppFolderAsync();
    }
}
