using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.AppModels
{
    public abstract class BaseSerializedDataModel<TService>
        where TService : class, ISettingsModel
    {
        protected TService SettingsService { get; } = Ioc.Default.GetRequiredService<TService>();

        protected virtual async Task<bool> EnsureSettingsLoaded(CancellationToken cancellationToken)
        {
            if (!SettingsService.IsAvailable)
                return await SettingsService.LoadSettingsAsync(cancellationToken);

            return true;
        }
    }
}
