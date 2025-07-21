using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IPrinterService>, Inject<IClipboardService>, Inject<IShareService>]
    [Bindable(true)]
    public sealed partial class RecoveryPreviewControlViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private string? _VaultId;
        [ObservableProperty] private string? _RecoveryKey;

        public RecoveryPreviewControlViewModel()
        {
            ServiceProvider = DI.Default;
        }

        [RelayCommand]
        private async Task ExportAsync(string? exportOption, CancellationToken cancellationToken)
        {
            _ = Title ?? throw new ArgumentNullException(nameof(Title));
            _ = RecoveryKey ?? throw new ArgumentNullException(nameof(Title));

            switch (exportOption?.ToLowerInvariant())
            {
                case "print":
                {
                    SynchronizationContext.Current.PostOrExecute(async _ =>
                    {
                        if (await PrinterService.IsSupportedAsync())
                            await PrinterService.PrintRecoveryKeyAsync(Title, VaultId, RecoveryKey);
                    });
                    break;
                }

                case "copy":
                {
                    if (await ClipboardService.IsSupportedAsync())
                        await ClipboardService.SetTextAsync(RecoveryKey, cancellationToken);

                    break;
                }

                case "share":
                {
                    await ShareService.ShareTextAsync(RecoveryKey, "RecoveryKey".ToLocalized());
                    break;
                }
            }
        }
    }
}
