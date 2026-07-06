using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Controls.Transfer;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class TransferExtensions
    {
        public static bool IsPickingItems(this TransferViewModel transferViewModel)
        {
            return transferViewModel is { IsVisible: true, TransferType: TransferType.Select };
        }

        public static async Task HideAsync(this TransferViewModel transferViewModel)
        {
            // An error banner is in control of its own dismissal (see TransferViewModel.ReportErrorAsync)
            if (transferViewModel.IsErrorVisible)
                return;

            transferViewModel.IsPickingFolder = false;
            transferViewModel.IsVisible = false;
            await Task.Delay(350);
            transferViewModel.IsProgressing = false;
        }

        public static async Task TransferAsync<TTransferred>(
            this TransferViewModel transferViewModel,
            IEnumerable<TTransferred> items,
            Func<TTransferred, IProgress<IStorable>, CancellationToken, Task> callback,
            CancellationToken cancellationToken = default)
            where TTransferred : IStorable
        {
            await TransferAsync(transferViewModel, items, callback, x => x.Name, cancellationToken);
        }

        public static async Task TransferAsync<TTransferred>(
            this TransferViewModel transferViewModel,
            IEnumerable<TTransferred> items,
            Func<TTransferred, IProgress<IStorable>, CancellationToken, Task> callback,
            Func<TTransferred, string> itemName,
            CancellationToken cancellationToken = default)
        {
            var collection = items.ToOrAsCollection();
            transferViewModel.ClearError();
            transferViewModel.IsProgressing = true;
            transferViewModel.IsVisible = true;
            transferViewModel.Report(new(0, collection.Count, collection.Count));
            var counter = 0;
            var reporter = new Progress<IStorable>(x =>
            {
                counter++;
                transferViewModel.Report(new(counter, 0, x.Name));
            });

            var failedCount = 0;
            var index = 0;
            foreach (var item in collection)
            {
                cancellationToken.ThrowIfCancellationRequested();
                transferViewModel.Report(new(index++, collection.Count, itemName(item)));

                try
                {
                    await callback(item, reporter, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    // A failed item must not abandon the remaining ones
                    failedCount++;
                }
            }

            transferViewModel.Report(new(collection.Count, collection.Count, string.Empty));
            await Task.Delay(1000, CancellationToken.None);
            await transferViewModel.HideAsync();

            if (failedCount > 0)
                await transferViewModel.ReportErrorAsync("TransferItemsFailedPlural".ToLocalized(failedCount));
        }

        public static async Task TransferAsync<TTransferred>(
            this TransferViewModel transferViewModel,
            IEnumerable<TTransferred> items,
            Func<TTransferred, CancellationToken, Task> callback,
            CancellationToken cancellationToken = default)
            where TTransferred : IStorable
        {
            await TransferAsync(transferViewModel, items, callback, x => x.Name, cancellationToken);
        }

        public static async Task TransferAsync<TTransferred>(
            this TransferViewModel transferViewModel,
            IEnumerable<TTransferred> items,
            Func<TTransferred, CancellationToken, Task> callback,
            Func<TTransferred, string> itemName,
            CancellationToken cancellationToken = default)
        {
            var collection = items.ToOrAsCollection();
            transferViewModel.ClearError();
            transferViewModel.IsProgressing = true;
            transferViewModel.IsVisible = true;
            transferViewModel.Report(new(0, collection.Count, collection.Count));

            var failedCount = 0;
            var index = 0;
            foreach (var item in collection)
            {
                cancellationToken.ThrowIfCancellationRequested();
                transferViewModel.Report(new(index++, collection.Count, itemName(item)));

                try
                {
                    await callback(item, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    // A failed item must not abandon the remaining ones
                    failedCount++;
                }
            }

            transferViewModel.Report(new(collection.Count, collection.Count, string.Empty));
            await Task.Delay(1000, CancellationToken.None);
            await transferViewModel.HideAsync();

            if (failedCount > 0)
                await transferViewModel.ReportErrorAsync("TransferItemsFailedPlural".ToLocalized(failedCount));
        }

        public static async Task PerformOperationAsync(this TransferViewModel transferViewModel, Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
        {
            var uiShown = false;
            var isCompleted = false;
            using var showUiCts = new CancellationTokenSource();

            try
            {
                transferViewModel.Title = transferViewModel.TransferType switch
                {
                    TransferType.Save => "Saving".ToLocalized(),
                    TransferType.Load => "Loading".ToLocalized(),
                    TransferType.Extract => "Extracting".ToLocalized(),
                    _ => string.Empty
                };
                transferViewModel.CanCancel = cancellationToken != CancellationToken.None;
                transferViewModel.IsProgressing = true;

                // Start a task that will show the UI after a delay if the operation is still running
                _ = ShowUiAfterDelayAsync(500, showUiCts.Token);

                // Run the operation and wait for it to complete
                await operation(cancellationToken);

                // Cancel the delayed UI show if operation completed quickly
                await showUiCts.CancelAsync();
                if (uiShown)
                {
                    transferViewModel.Title = "TransferDone".ToLocalized();
                    await Task.Delay(300, CancellationToken.None); // Allow user to see the "Done" message
                }
            }
            catch (OperationCanceledException)
            {
                await showUiCts.CancelAsync();
                if (uiShown)
                {
                    transferViewModel.CanCancel = false;
                    transferViewModel.Title = "Cancelling".ToLocalized();
                }
            }
            finally
            {
                isCompleted = true;
                await HideAsync(transferViewModel);
            }

            return;

            async Task ShowUiAfterDelayAsync(int delayMs, CancellationToken ct)
            {
                try
                {
                    await Task.Delay(delayMs, ct);
                    if (isCompleted)
                        return;

                    uiShown = true;
                    transferViewModel.IsVisible = true;
                }
                catch (OperationCanceledException)
                {
                    // Operation completed before delay, don't show UI
                }
            }
        }
    }
}
