using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Transfer
{
    [Bindable(true)]
    public sealed partial class TransferViewModel : ObservableObject, IViewable, IProgress<TotalProgress>, IFolderPicker
    {
        private readonly BrowserViewModel _browserViewModel;
        private readonly SynchronizationContext? _synchronizationContext;
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
            _synchronizationContext = SynchronizationContext.Current;
        }

        /// <inheritdoc/>
        public void Report(TotalProgress value)
        {
            // Progress is reported from worker threads, but Title is UI-bound.
            // Marshal the update back to the context this view model was created on
            if (_synchronizationContext != SynchronizationContext.Current)
            {
                _synchronizationContext.PostOrExecute(static state =>
                {
                    var (self, progress) = ((TransferViewModel, TotalProgress))state!;
                    self.ReportCore(progress);
                }, (this, value));

                return;
            }

            ReportCore(value);
        }

        private void ReportCore(TotalProgress value)
        {
            if (value.Achieved >= value.Total && value.Total > 0)
            {
                Title = "TransferDone".ToLocalized();
                return;
            }

            Title = TransferType switch
            {
                TransferType.Copy => "CopyingItemsPlural".ToLocalized(GetInterpolation()),
                TransferType.Move => "MovingItemsPlural".ToLocalized(GetInterpolation()),
                TransferType.Delete => "DeletingItemsPlural".ToLocalized(GetInterpolation()),
                TransferType.Extract => "ExtractingItemsPlural".ToLocalized(GetInterpolation()),
                _ => "Loading".ToLocalized()
            };
            return;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            object GetInterpolation()
            {
                return new { value.State, value.Achieved, value.Total };
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
                if (_tcs?.TrySetCanceled(CancellationToken.None) ?? false)
                    await this.HideAsync();

                // Clear the completed source, otherwise a later Cancel would be routed
                // to the finished pick instead of cancelling the running transfer
                _tcs = null;
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
                try
                {
                    CanCancel = false;
                    Title = "Cancelling".ToLocalized();
                    await _cts.CancelAsync();
                }
                catch (ObjectDisposedException)
                {
                    // CTS was already disposed, just hide
                    await this.HideAsync();
                }
            }
        }
    }
}
