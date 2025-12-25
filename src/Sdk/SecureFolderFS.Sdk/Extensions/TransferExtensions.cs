using System;
using System.Collections.Generic;
using System.Linq;
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
            transferViewModel.IsPickingFolder = false;
            transferViewModel.IsVisible = false;
            await Task.Delay(350);
            transferViewModel.IsProgressing = false;
        }

        public static async Task TransferAsync<TStorable>(
            this TransferViewModel transferViewModel,
            IEnumerable<TStorable> items,
            Func<TStorable, IProgress<IStorable>, CancellationToken, Task> callback,
            CancellationToken cancellationToken = default)
        {
            var collection = items.ToOrAsCollection();
            transferViewModel.IsProgressing = true;
            transferViewModel.IsVisible = true;
            transferViewModel.Report(new(0, 0));
            var counter = 0;
            var reporter = new Progress<IStorable>(_ =>
            {
                counter++;
                transferViewModel.Report(new(counter, 0));
            });

            for (var i = 0; i < collection.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = collection.ElementAt(i);
                await callback(item, reporter, cancellationToken);
            }

            transferViewModel.Title = "TransferDone".ToLocalized();
            await Task.Delay(1000, CancellationToken.None);
            await transferViewModel.HideAsync();
        }

        public static async Task TransferAsync<TStorable>(
            this TransferViewModel transferViewModel,
            IEnumerable<TStorable> items,
            Func<TStorable, CancellationToken, Task> callback,
            CancellationToken cancellationToken = default)
            where TStorable : IStorable
        {
            var collection = items.ToOrAsCollection();
            transferViewModel.IsProgressing = true;
            transferViewModel.IsVisible = true;
            transferViewModel.Report(new(0, collection.Count));

            for (var i = 0; i < collection.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = collection.ElementAt(i);
                await callback(item, cancellationToken);
                transferViewModel.Report(new((i + 1), collection.Count));
            }

            await Task.Delay(1000, CancellationToken.None);
            await transferViewModel.HideAsync();
        }

        public static async Task PerformOperationAsync(this TransferViewModel transferViewModel, Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
        {
            var uiShown = false;
            var showUiCts = new CancellationTokenSource();

            try
            {
                transferViewModel.Title = transferViewModel.TransferType switch
                {
                    TransferType.Save => "Saving".ToLocalized(),
                    TransferType.Load => "Loading".ToLocalized(),
                    _ => string.Empty,
                };
                transferViewModel.CanCancel = cancellationToken != CancellationToken.None;
                transferViewModel.IsProgressing = true;

                // Start a task that will show the UI after a delay if the operation is still running
                _ = ShowUiAfterDelayAsync(500, showUiCts.Token);

                // Run the operation and wait for it to complete
                var operationTask = operation(cancellationToken);
                await operationTask;

                // Cancel the delayed UI show if operation completed quickly
                await showUiCts.CancelAsync();
                if (uiShown)
                {
                    transferViewModel.Title = "TransferDone".ToLocalized();
                    await Task.Delay(300); // Allow user to see the "Done" message
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
                showUiCts.Dispose();
                await HideAsync(transferViewModel);
            }

            return;

            async Task ShowUiAfterDelayAsync(int delayMs, CancellationToken ct)
            {
                try
                {
                    await Task.Delay(delayMs, ct);
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
