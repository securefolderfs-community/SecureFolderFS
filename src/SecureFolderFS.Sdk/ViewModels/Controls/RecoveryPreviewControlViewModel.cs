using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IPrinterService>, Inject<IThreadingService>]
    [Bindable(true)]
    public sealed partial class RecoveryPreviewControlViewModel : ObservableObject
    {
        [ObservableProperty] private string? _VaultId;
        [ObservableProperty] private string? _VaultName;
        [ObservableProperty] private string? _MasterKey;

        public RecoveryPreviewControlViewModel()
        {
            ServiceProvider = DI.Default;
        }

        [RelayCommand]
        private async Task ExportAsync(string? exportHint, CancellationToken cancellationToken)
        {
            _ = VaultName ?? throw new ArgumentNullException(nameof(VaultName));
            switch (exportHint?.ToLowerInvariant())
            {
                case "print":
                {
                    await ThreadingService.ChangeThreadAsync();
                    if (await PrinterService.IsSupportedAsync())
                        await PrinterService.PrintMasterKeyAsync(VaultName, VaultId, MasterKey);

                    break;
                }

                case "share":
                {
                    // TODO: Share
                    break;
                }

                default: break;
            }
        }
    }
}
