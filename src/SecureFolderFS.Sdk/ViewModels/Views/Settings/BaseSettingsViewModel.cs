using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ISettingsService>]
    public abstract partial class BaseSettingsViewModel : BaseDesignationViewModel, IAsyncInitialize
    {
        protected IAppSettings AppSettings => SettingsService.AppSettings;

        protected IUserSettings UserSettings => SettingsService.UserSettings;

        protected BaseSettingsViewModel()
        {
            ServiceProvider = Ioc.Default;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
