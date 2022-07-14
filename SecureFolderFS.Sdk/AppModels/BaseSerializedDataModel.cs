using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.AppModels
{
    public abstract class BaseSerializedDataModel<TService>
        where TService : class, ISettingsModel
    {
        protected internal bool settingsLoaded;

        protected internal TService SettingsService { get; } = Ioc.Default.GetRequiredService<TService>();

        protected internal virtual async Task<bool> EnsureSettingsLoaded(CancellationToken cancellationToken)
        {
            settingsLoaded |= !settingsLoaded && await SettingsService.LoadSettingsAsync(cancellationToken);
            return settingsLoaded;
        }
    }
}
