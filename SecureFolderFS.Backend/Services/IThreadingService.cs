namespace SecureFolderFS.Backend.Services
{
    public interface IThreadingService
    {
        Task ExecuteOnUiThreadAsync(Action action);

        Task<TResult?> ExecuteOnUiThreadAsync<TResult>(Func<TResult?> func);
    }
}
