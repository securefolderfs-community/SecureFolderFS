namespace SecureFolderFS.Backend.Services
{
    public interface IApplicationService
    {
        void CloseApplication();

        Task OpenUriAsync(Uri uri);
    }
}
