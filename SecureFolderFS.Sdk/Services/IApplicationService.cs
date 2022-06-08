namespace SecureFolderFS.Sdk.Services
{
    public interface IApplicationService
    {
        Version GetAppVersion();

        void CloseApplication();

        Task OpenUriAsync(Uri uri);

        Task OpenAppFolderAsync();
    }
}
