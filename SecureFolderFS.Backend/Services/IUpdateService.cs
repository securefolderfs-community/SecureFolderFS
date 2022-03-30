using SecureFolderFS.Backend.Enums;

namespace SecureFolderFS.Backend.Services
{
    public interface IUpdateService
    {
        Task<bool> AreAppUpdatesSupportedAsync();

        Task<bool> IsNewUpdateAvailableAsync();

        Task<AppUpdateResult> UpdateAppAsync(IProgress<double>? progress);
    }
}
