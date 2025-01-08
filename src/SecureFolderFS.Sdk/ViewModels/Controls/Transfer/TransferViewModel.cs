using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Views.Browser;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Transfer
{
    public sealed partial class TransferViewModel : ObservableObject, IViewable, IProgress<TotalProgress>
    {
        private readonly BrowserViewModel _browserViewModel;
        private TaskCompletionSource<FolderViewModel?>? _tcs;
        private CancellationTokenSource? _cts;
        
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsVisible;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private TransferType _TransferType;

        public TransferViewModel(BrowserViewModel browserViewModel)
        {
            _browserViewModel = browserViewModel;
        }

        /// <inheritdoc/>
        public void Report(TotalProgress value)
        {
            if (value.Achieved >= value.Total)
            {
                Title = "Done";
                return;
            }
            
            Title = $"{TransferType switch
            {
                TransferType.Copy => "Copying",
                TransferType.Move => "Moving",
                _ => string.Empty,
            }} {GetItemsCount(value)} item(s)";

            static string GetItemsCount(TotalProgress totalProgress)
            {
                return totalProgress switch
                {
                    { Achieved: < 0 } => totalProgress.Total.ToString(),
                    { Total: < 0 } => totalProgress.Achieved.ToString(),
                    _ => $"{totalProgress.Achieved}/{totalProgress.Total}"
                };
            }
        }

        public async Task<FolderViewModel?> SelectFolderAsync(TransferType transferType, CancellationTokenSource? cts)
        {
            try
            {
                _tcs?.TrySetCanceled();
                _tcs = new();
                _cts = cts;
                TransferType = transferType;
                Title = "Choose destination folder";
                IsVisible = true;
                return await _tcs.Task;
            }
            finally
            {
                _tcs = null;
                _cts = null;
            }
        }

        [RelayCommand]
        private void Confirm()
        {
            // Only used for confirming the destination folder
            _tcs?.TrySetResult(_browserViewModel.CurrentFolder);
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            if (_tcs is not null)
            {
                _tcs?.TrySetCanceled();
                IsVisible = false;
                return;
            }

            // Cancel here using the provided CancellationTokenSource
            if (_cts is not null)
            {
                Title = "Cancelling";
                await _cts.CancelAsync();

                IsVisible = false;
            }
        }
    }
}
