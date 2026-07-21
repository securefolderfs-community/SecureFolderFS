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
        private const int ERROR_DISPLAY_DURATION_MS = 5000;

        private readonly BrowserViewModel? _browserViewModel;
        private readonly SynchronizationContext? _synchronizationContext;
        private TaskCompletionSource<IFolder?>? _tcs;
        private CancellationTokenSource? _cts;
        private CancellationTokenSource? _errorCts;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _CanCancel;
        [ObservableProperty] private bool _IsVisible;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private bool _IsPickingFolder;
        [ObservableProperty] private bool _IsErrorVisible;
        [ObservableProperty] private TransferType _TransferType;

        /// <summary>
        /// Gets or sets an optional override for resolving the destination folder when the user
        /// confirms a folder pick. Used by hosts that share one transfer view model across multiple
        /// browser instances (e.g. tabbed browsing), where the destination is whichever browser is
        /// currently in view rather than the one this view model was created with.
        /// </summary>
        public Func<IFolder?>? DestinationResolver { get; set; }

        public TransferViewModel(BrowserViewModel? browserViewModel = null)
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

        /// <summary>
        /// Shows an indeterminate progress state, e.g. while item sizes are being calculated before a transfer.
        /// </summary>
        /// <param name="title">The title to display while the operation is running.</param>
        public void ShowIndeterminate(string title)
        {
            ClearError();
            Title = title;
            IsProgressing = true;
            IsVisible = true;
        }

        /// <summary>
        /// Dismisses any operation UI and shows <paramref name="message"/> as a transient error banner.
        /// The banner disappears on its own or when dismissed by the user.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        /// <returns>A <see cref="Task"/> that completes once the banner is shown.</returns>
        public async Task ReportErrorAsync(string message)
        {
            ClearError();
            _errorCts = new();
            var token = _errorCts.Token;

            // Finish and dismiss the current operation UI before revealing the error
            if (IsVisible)
                await this.HideAsync();

            if (token.IsCancellationRequested)
                return;

            Title = message;
            IsErrorVisible = true;
            IsProgressing = false;
            CanCancel = true;
            IsVisible = true;

            _ = DismissErrorLaterAsync(token);
        }

        private async Task DismissErrorLaterAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(ERROR_DISPLAY_DURATION_MS, token);
                IsErrorVisible = false;
                await this.HideAsync();
            }
            catch (OperationCanceledException)
            {
                // A newer operation or an explicit dismissal took over the control
            }
        }

        /// <summary>
        /// Clears the error state, if any, allowing the control to display a new operation.
        /// </summary>
        public void ClearError()
        {
            _errorCts?.TryCancel();
            _errorCts?.Dispose();
            _errorCts = null;
            IsErrorVisible = false;
        }

        private void ReportCore(TotalProgress value)
        {
            if (IsErrorVisible)
                return;

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
            ClearError();
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
            _tcs?.TrySetResult(DestinationResolver is not null
                ? DestinationResolver()
                : _browserViewModel?.CurrentFolder?.Folder);
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            if (IsErrorVisible)
            {
                // Dismiss the error banner since there is no operation left to cancel
                ClearError();
                await this.HideAsync();
                return;
            }

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
