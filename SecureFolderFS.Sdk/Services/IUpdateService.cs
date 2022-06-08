using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.Services
{
    public interface IUpdateService
    {
        Task<bool> AreAppUpdatesSupportedAsync();

        Task<bool> IsNewUpdateAvailableAsync();

        Task<AppUpdateResult> UpdateAppAsync(IProgress<double>? progress);
    }
}
