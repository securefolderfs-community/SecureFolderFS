using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Transfer
{
    [Bindable(true)]
    public sealed partial class TransferViewModel : ObservableObject, IViewable, IProgress<TotalProgress>, IFolderPicker
    {
        private readonly BrowserViewModel _browserViewModel;
        private TaskCompletionSource<IFolder?>? _tcs;
        private CancellationTokenSource? _cts;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _CanCancel;
        [ObservableProperty] private bool _IsVisible;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private bool _IsPickingFolder;
        [ObservableProperty] private TransferType _TransferType;

        public TransferViewModel(BrowserViewModel browserViewModel)
        {
            _browserViewModel = browserViewModel;
        }

        /// <inheritdoc/>
        public void Report(TotalProgress value)
        {
            if (value.Achieved >= value.Total && value.Total > 0)
            {
                Title = "TransferDone".ToLocalized();
                return;
            }

            Title = $"{TransferType switch
            {
                TransferType.Copy => "Copying",
                TransferType.Move => "Moving",
                _ => string.Empty,
            }} {GetItemsCount(value)} item(s)";

            return;

            static string GetItemsCount(TotalProgress totalProgress)
            {
                return totalProgress switch
                {
                    { Achieved: < 0 } => totalProgress.Total.ToString(),
                    { Total: <= 0 } => totalProgress.Achieved.ToString(),
                    _ => $"{totalProgress.Achieved}/{totalProgress.Total}"
                };
            }
        }

        public CancellationTokenSource GetCancellation(CancellationToken? linkToken = null)
        {
            _cts?.Dispose();
            _cts = linkToken is not null
                ? CancellationTokenSource.CreateLinkedTokenSource(linkToken.Value)
                : new CancellationTokenSource();

            CanCancel = true;
            return _cts;
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(PickerOptions? options, bool offerPersistence = true, CancellationToken cancellationToken = default)
        {
            try
            {
                IsPickingFolder = true;
                _tcs?.TrySetCanceled(CancellationToken.None);
                _tcs = new TaskCompletionSource<IFolder?>();

                if (options is TransferOptions transferOptions)
                    TransferType = transferOptions.TransferType;

                Title = "ChooseDestinationFolder".ToLocalized();
                IsProgressing = false;
                IsVisible = true;

                return await _tcs.Task;
            }
            finally
            {
                IsPickingFolder = false;
            }
        }

        [RelayCommand]
        private void Confirm()
        {
            // Only used for confirming the destination folder
            _tcs?.TrySetResult(_browserViewModel.CurrentFolder?.Folder);
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            if (_tcs is not null)
            {
                _tcs.TrySetCanceled(CancellationToken.None);
                await this.HideAsync();
            }
            else if (_cts is not null)
            {
                CanCancel = false;
                Title = "Cancelling".ToLocalized();
                await _cts.CancelAsync();
                await this.HideAsync();
            }
        }
    }
}
