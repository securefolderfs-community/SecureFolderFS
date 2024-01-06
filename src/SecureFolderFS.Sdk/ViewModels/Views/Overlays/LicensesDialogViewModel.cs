using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IApplicationService>]
    public sealed partial class LicensesDialogViewModel : DialogViewModel, IAsyncInitialize
    {
        public ObservableCollection<LicenseViewModel> Licenses { get; }

        public LicensesDialogViewModel()
        {
            ServiceProvider = Ioc.Default;
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