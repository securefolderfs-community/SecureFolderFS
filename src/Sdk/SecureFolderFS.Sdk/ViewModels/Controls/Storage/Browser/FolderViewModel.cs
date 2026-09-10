using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels.Sorters;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser
{
    [Inject<ISettingsService>, Inject<ILogger>]
    [Bindable(true)]
    public partial class FolderViewModel : BrowserItemViewModel, IViewDesignation
    {
        private const int LISTING_BATCH_SIZE = 32;

        private CancellationTokenSource? _listingCts;

        /// <summary>
        /// Gets the folder associated with this view model.
        /// </summary>
        public IFolder Folder { get; protected set; }

        /// <summary>
        /// Gets the items in this folder.
        /// </summary>
        public ObservableCollection<BrowserItemViewModel> Items { get; }

        /// <summary>
        /// Gets the selected items in this folder.
        /// </summary>
        public ObservableCollection<BrowserItemViewModel> SelectedItems { get; }

        /// <inheritdoc/>
        public override IStorable Inner => Folder;

        public FolderViewModel(IFolder folder, BrowserViewModel browserViewModel, FolderViewModel? parentFolder)
            : base(browserViewModel, parentFolder)
        {
            ServiceProvider = DI.Default;
            Folder = folder;
            Title = folder.Name;
            SelectedItems = new();
            Items = new();
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            LastModified = await Folder.GetDateModifiedAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual void OnAppearing()
        {
            // Apply adaptive layout when back to a folder
            if (!Items.IsEmpty() && SettingsService.UserSettings.IsAdaptiveLayoutEnabled && BrowserViewModel.TransferViewModel is { IsPickingFolder: false })
                ApplyAdaptiveLayout();
        }

        /// <inheritdoc/>
        public virtual void OnDisappearing()
        {
        }

        /// <summary>
        /// Asynchronously retrieves and lists the contents of the current folder, organizing them into the observable collection of items.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task ListContentsAsync(CancellationToken cancellationToken = default)
        {
            // Cancel any listing already in flight
            if (_listingCts is not null)
                await _listingCts.CancelAsync();

            _listingCts?.Dispose();
            _listingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _listingCts.Token;

            // The existing contents are only cleared once the first batch is ready, so a
            // canceled or failed enumeration that produced nothing leaves them intact
            var cleared = false;

            try
            {
                var scope = Logger.GetPerformanceScope();

                var isPickingFolder = BrowserViewModel.IsPickingFolder;
                var sorter = BrowserViewModel.Layouts.GetSorter();

                // Sorting by date needs the modification date before the item is inserted;
                // for other sorters it is loaded lazily when the item scrolls into view
                var needsDates = sorter is DateSorter;

                var batch = new List<BrowserItemViewModel>(LISTING_BATCH_SIZE);
                await foreach (var item in Folder.GetItemsAsync(isPickingFolder ? StorableType.Folder : StorableType.All, token))
                {
                    if (isPickingFolder && item is not IFolder)
                        continue;

                    var itemViewModel = (BrowserItemViewModel)(item switch
                    {
                        IFile file => new FileViewModel(file, BrowserViewModel, this),
                        IFolder folder => new FolderViewModel(folder, BrowserViewModel, this),
                        _ => throw new ArgumentOutOfRangeException(nameof(item))
                    });

                    if (needsDates)
                    {
                        itemViewModel.LastModified = item switch
                        {
                            IFile file => await file.GetDateModifiedAsync(token),
                            IFolder folder => await folder.GetDateModifiedAsync(token),
                            _ => null
                        };
                    }

                    batch.Add(itemViewModel);
                    if (batch.Count < LISTING_BATCH_SIZE)
                        continue;

                    FlushBatch(batch, sorter, ref cleared);

                    // Let the UI render the batch and stay responsive even when
                    // the enumeration completes synchronously (e.g. local storage)
                    await Task.Yield();
                    token.ThrowIfCancellationRequested();
                }

                token.ThrowIfCancellationRequested();
                FlushBatch(batch, sorter, ref cleared);

                // An empty enumeration never flushed - the folder no longer has any items
                if (!cleared)
                {
                    SelectedItems.Clear();
                    Items.DisposeAll();
                    Items.Clear();
                }

                // Apply adaptive layout
                if (SettingsService.UserSettings.IsAdaptiveLayoutEnabled && BrowserViewModel.TransferViewModel is { IsPickingFolder: false })
                    ApplyAdaptiveLayout();

                Logger.LogPerformance(scope, minThresholdMs: 200);
            }
            catch (OperationCanceledException)
            {
                // A newer listing superseded this one (or the caller canceled)
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to list the contents of a folder.");
                if (BrowserViewModel.TransferViewModel is { } transferViewModel)
                    await transferViewModel.ReportErrorAsync("FolderLoadFailed".ToLocalized());
            }
        }

        private void FlushBatch(List<BrowserItemViewModel> batch, IItemSorter<BrowserItemViewModel> sorter, ref bool cleared)
        {
            if (batch.Count == 0)
                return;

            if (!cleared)
            {
                SelectedItems.Clear();
                Items.DisposeAll();
                Items.Clear();
                cleared = true;
            }

            foreach (var itemViewModel in batch)
                Items.Insert(itemViewModel, sorter);

            batch.Clear();
        }

        /// <inheritdoc/>
        protected override void UpdateStorable(IStorable storable)
        {
            Folder = (IFolder)storable;
        }

        /// <inheritdoc/>
        protected override async Task OpenAsync(CancellationToken cancellationToken)
        {
            if (Items.IsEmpty())
                _ = ListContentsAsync(cancellationToken);

            await BrowserViewModel.InnerNavigator.NavigateAsync(this);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            // Stop an in-flight listing so it does not keep mutating Items after disposal
            _listingCts?.TryCancel();
            _listingCts?.Dispose();
            _listingCts = null;
            base.Dispose();
        }

        private void ApplyAdaptiveLayout()
        {
            // The user picked a layout manually - respect their choice
            if (BrowserViewModel.Layouts.IsAdaptiveLayoutSuspended)
                return;

            var itemCount = Items.Count;
            if (itemCount == 0)
                return;

            // Classify each item once
            int folders = 0, images = 0, media = 0, documents = 0;
            foreach (var item in Items)
            {
                if (item.Inner is IFolder)
                {
                    folders++;
                    continue;
                }

                switch (FileTypeHelper.GetTypeHint(item.Inner))
                {
                    case TypeHint.Image: images++; break;
                    case TypeHint.Media: media++; break;
                    case TypeHint.Document or TypeHint.Plaintext: documents++; break;
                }
            }

            var folderPercentage = 100f * folders / itemCount;
            var imagePercentage = 100f * images / itemCount;
            var mediaPercentage = 100f * media / itemCount;
            var documentPercentage = 100f * documents / itemCount;
            var otherPercentage = 100f - (folderPercentage + imagePercentage + mediaPercentage + documentPercentage);

            var galleryView = BrowserViewModel.Layouts.ViewOptions.FirstOrDefault(x => x.Id == "GalleryView");
            var columnView = BrowserViewModel.Layouts.ViewOptions.FirstOrDefault(x => x.Id == "ColumnView");
            var listView = BrowserViewModel.Layouts.ViewOptions.FirstOrDefault(x => x.Id == "ListView");
            var gridView = BrowserViewModel.Layouts.ViewOptions.FirstOrDefault(x => x.Id == "GridView");

            if (imagePercentage + mediaPercentage >= 90f)
            {
                // GalleryView or GridView
                BrowserViewModel.Layouts.ApplyAdaptiveViewOption(SettingsService.UserSettings.AreThumbnailsEnabled
                    ? galleryView
                    : gridView);
            }
            else if (imagePercentage + mediaPercentage >= 70f)
            {
                // GridView
                BrowserViewModel.Layouts.ApplyAdaptiveViewOption(gridView);
            }
            else if (documentPercentage + folderPercentage >= 50f)
            {
                // GridView
                BrowserViewModel.Layouts.ApplyAdaptiveViewOption(gridView);
            }
            else if (otherPercentage + folderPercentage >= 50f)
            {
                // ColumnView
                BrowserViewModel.Layouts.ApplyAdaptiveViewOption(columnView);
            }
            else
            {
                // ListView
                BrowserViewModel.Layouts.ApplyAdaptiveViewOption(listView);
            }
        }
    }
}
