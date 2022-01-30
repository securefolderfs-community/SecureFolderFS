namespace SecureFolderFS.Backend.Services
{
    public interface IThreadingService
    {
        Task ExecuteOnUiThreadAsync(Action action);
    }
}
