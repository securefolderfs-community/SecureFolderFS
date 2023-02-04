using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Dialogs
{
    public sealed class LicensesDialogViewModel : DialogViewModel, IAsyncInitialize
    {
        private IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        public ObservableCollection<LicenseViewModel> Licenses { get; }

        public LicensesDialogViewModel()
        {
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