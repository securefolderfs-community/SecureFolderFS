using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ISettingsService>]
    [Bindable(true)]
    public abstract partial class BaseSettingsViewModel : BaseDesignationViewModel, IAsyncInitialize
    {
        protected IAppSettings AppSettings => SettingsService.AppSettings;

        protected IUserSettings UserSettings => SettingsService.UserSettings;

        protected BaseSettingsViewModel()
        {
            ServiceProvider = DI.Default;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
