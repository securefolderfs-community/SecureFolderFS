using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.ViewModels.Controls.Components;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IApplicationService>]
    [Bindable(true)]
    public sealed partial class LicensesOverlayViewModel : OverlayViewModel, IAsyncInitialize
    {
        public ObservableCollection<LicenseViewModel> Licenses { get; }

        public LicensesOverlayViewModel()
        {
            ServiceProvider = DI.Default;
            Licenses = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in ApplicationService.GetLicensesAsync(cancellationToken))
            {
                Licenses.Add(item);
            }
        }
    }
}