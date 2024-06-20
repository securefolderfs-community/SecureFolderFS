using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IPrinterService>, Inject<IThreadingService>]
    public sealed partial class RecoveryPreviewControlViewModel : ObservableObject
    {
        [ObservableProperty] private string? _VaultId;
        [ObservableProperty] private string? _VaultName;
        [ObservableProperty] private string? _MasterKey;

        public RecoveryPreviewControlViewModel()
        {
            ServiceProvider = Ioc.Default;
        }

        [RelayCommand]
        private async Task ExportAsync(string? exportHint, CancellationToken cancellationToken)
        {
            _ = VaultName ?? throw new ArgumentNullException(nameof(VaultName));

            await ThreadingService.ChangeThreadAsync();
            if (await PrinterService.IsSupportedAsync())
                await PrinterService.PrintMasterKeyAsync(VaultName, VaultId, _MasterKey);
        }
    }
}
