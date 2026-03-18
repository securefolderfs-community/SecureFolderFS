using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class BrowserSearchOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private const int SearchResultBatchSize = 24;
        private const int ThumbnailWorkerCount = 2;

        private readonly ISearchModel _searchModel;
        private readonly ThumbnailCacheModel _thumbnailCache;
        private readonly Func<SearchBrowserItemViewModel, CancellationToken, Task> _openSearchResultAsync;
        private CancellationTokenSource? _searchCts;
        private readonly IFolder _searchedFolder;

        [ObservableProperty] private string? _Query;
        [ObservableProperty] private bool _IncludeChildDirectories;
        [ObservableProperty] private bool _IsSearching;
        [ObservableProperty] private bool _HasSearched;

        public ObservableCollection<SearchBrowserItemViewModel> SearchResults { get; }

        public event EventHandler? CloseRequested;

        public BrowserSearchOverlayViewModel(
            IFolder searchedFolder,
            ISearchModel searchModel,
            ThumbnailCacheModel thumbnailCache,
            Func<SearchBrowserItemViewModel, CancellationToken, Task> openSearchResultAsync)
        {
            _searchModel = searchModel;
            _searchedFolder = searchedFolder;
            _thumbnailCache = thumbnailCache;
            _openSearchResultAsync = openSearchResultAsync;
            SearchResults = new();
            IncludeChildDirectories = true;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Title = _searchedFolder.Id == Path.DirectorySeparatorChar.ToString() || _searchedFolder.Id == Path.AltDirectorySeparatorChar.ToString()
                ? "SearchIn".ToLocalized("RootFolder".ToLocalized())
                : "SearchIn".ToLocalized(_searchedFolder.Name);

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task SubmitQueryAsync(string? query, CancellationToken cancellationToken)
        {
            Query = query ?? Query;
            if (string.IsNullOrWhiteSpace(Query))
                return;

            CancelSearch();
            SearchResults.DisposeAll();

            SearchResults.Clear();
            HasSearched = true;
            IsSearching = true;

            // Capture the UI context here, while we're still on the UI thread.
            var uiContext = SynchronizationContext.Current;

            Channel<SearchBrowserItemViewModel>? thumbnailChannel = null;
            Task[] thumbnailWorkers = [];

            try
            {
                _searchCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var searchToken = _searchCts.Token;

                if (_searchModel is BrowserFolderSearchModel folderSearchModel)
                    folderSearchModel.IncludeChildDirectories = IncludeChildDirectories;

                var pendingResults = new List<SearchBrowserItemViewModel>(SearchResultBatchSize);
                thumbnailChannel = Channel.CreateUnbounded<SearchBrowserItemViewModel>(new UnboundedChannelOptions()
                {
                    SingleReader = false,
                    SingleWriter = true
                });

                thumbnailWorkers = CreateThumbnailWorkers(thumbnailChannel.Reader, searchToken);

                // Hop off the UI thread once before the scan loop so that
                // scanner I/O and item construction never block the UI thread.
                await Task.Run(async () =>
                {
                    await foreach (var item in _searchModel.SearchAsync(Query, searchToken).ConfigureAwait(false))
                    {
                        if (item is not IStorable storable)
                            continue;

                        var searchItem =
                            new SearchBrowserItemViewModel(storable, _thumbnailCache, _openSearchResultAsync);
                        pendingResults.Add(searchItem);

                        if (searchItem.CanLoadThumbnail())
                            _ = thumbnailChannel.Writer.TryWrite(searchItem);

                        if (pendingResults.Count >= SearchResultBatchSize)
                            await FlushPendingResultsAsync(pendingResults, uiContext, searchToken)
                                .ConfigureAwait(false);
                    }

                    await FlushPendingResultsAsync(pendingResults, uiContext, searchToken).ConfigureAwait(false);
                }, searchToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // A new query superseded the previous one.
            }
            finally
            {
                thumbnailChannel?.Writer.TryComplete();

                if (thumbnailWorkers.Length != 0)
                {
                    try
                    {
                        await Task.WhenAll(thumbnailWorkers).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore cancellation while stopping workers.
                    }
                }

                // Marshal IsSearching = false back to the UI thread.
                await uiContext.PostOrExecuteAsync(() =>
                {
                    IsSearching = false;
                    return Task.CompletedTask;
                });
            }
        }

        private async Task FlushPendingResultsAsync(
            List<SearchBrowserItemViewModel> pendingResults,
            SynchronizationContext? uiContext,
            CancellationToken cancellationToken)
        {
            if (pendingResults.Count == 0)
                return;

            var batch = pendingResults.ToArray();
            pendingResults.Clear();

            await uiContext.PostOrExecuteAsync(() =>
            {
                foreach (var searchResult in batch)
                    SearchResults.Add(searchResult);
                return Task.CompletedTask;
            });

            // Yield after each batch so the collection can render and remain scrollable.
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
        }

        [RelayCommand]
        private async Task OpenSearchResultAsync(SearchBrowserItemViewModel? searchItem, CancellationToken cancellationToken)
        {
            if (searchItem is null)
                return;

            await _openSearchResultAsync(searchItem, cancellationToken);
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private Task[] CreateThumbnailWorkers(ChannelReader<SearchBrowserItemViewModel> channelReader, CancellationToken cancellationToken)
        {
            var workers = new Task[ThumbnailWorkerCount];
            for (var i = 0; i < workers.Length; i++)
                workers[i] = RunThumbnailWorkerAsync(channelReader, cancellationToken);

            return workers;
        }

        private static async Task RunThumbnailWorkerAsync(ChannelReader<SearchBrowserItemViewModel> channelReader, CancellationToken cancellationToken)
        {
            while (await channelReader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (channelReader.TryRead(out var itemViewModel))
                {
                    try
                    {
                        await itemViewModel.InitAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
        }

        private void CancelSearch()
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CancelSearch();
            SearchResults.DisposeAll();
        }
    }
}
