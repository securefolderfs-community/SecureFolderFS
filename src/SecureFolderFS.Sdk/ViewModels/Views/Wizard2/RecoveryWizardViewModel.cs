using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard2
{
    [Inject<IPrinterService>, Inject<IThreadingService>]
    public sealed partial class RecoveryWizardViewModel : BaseWizardViewModel
    {
        private readonly IFolder _folder;
        private readonly IDisposable _superSecret;

        [ObservableProperty] private string? _MasterKey;

        public RecoveryWizardViewModel(IFolder folder, IDisposable superSecret)
        {
            ServiceProvider = Ioc.Default;
            CanContinue = true;
            CanCancel = false;
            _folder = folder;
            _superSecret = superSecret;
        }

        /// <inheritdoc/>
        public override Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(CommonResult.Success);
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(CommonResult.Success);
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
        {
            _superSecret.Dispose();
            MasterKey = null;
        }

        [RelayCommand]
        private async Task PrintAsync(CancellationToken cancellationToken)
        {
            await ThreadingService.ChangeThreadAsync();

            if (await PrinterService.IsSupportedAsync())
                await PrinterService.PrintMasterKeyAsync(_superSecret, _folder.Name);
        }


        [RelayCommand]
        private void RevealMasterKey()
        {
            MasterKey ??= _superSecret.ToString();
        }
    }
}
