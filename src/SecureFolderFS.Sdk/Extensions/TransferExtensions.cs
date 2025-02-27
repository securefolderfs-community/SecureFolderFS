using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Transfer;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.Extensions
{
    public static class TransferExtensions
    {
        public static Task TransferAsync<TStorable>(
            this TransferViewModel transferViewModel,
            TStorable item,
            Func<TStorable, CancellationToken, Task> callback,
            CancellationToken cancellationToken = default)
        where TStorable : IStorable
        {
            return TransferAsync(transferViewModel, [ item ], callback, cancellationToken);
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

            await Task.Delay(1000);
            transferViewModel.IsProgressing = false;
            transferViewModel.IsVisible = false;
        }
    }
}
