using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OwlCore.Storage;
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

            try
            {
                var scope = Logger.GetPerformanceScope();

                // Enumerate before mutating the collection, so a canceled or failed
                // enumeration leaves the current contents intact
                var isPickingFolder = BrowserViewModel.IsPickingFolder;
                var items = await Folder.GetItemsAsync(isPickingFolder ? StorableType.Folder : StorableType.All, token).ToArrayAsyncImpl(cancellationToken: token);
                token.ThrowIfCancellationRequested();

                SelectedItems.Clear();
                Items.DisposeAll();
                Items.Clear();

                BrowserViewModel.Layouts.GetSorter().SortCollection(items.Where(x => !isPickingFolder || x is IFolder).Select(x => (BrowserItemViewModel)(x switch
                {
                    IFile file => new FileViewModel(file, BrowserViewModel, this),
                    IFolder folder => new FolderViewModel(folder, BrowserViewModel, this),
                    _ => throw new ArgumentOutOfRangeException(nameof(x))
                })), Items);

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
                // Only inform the user that the refresh failed and leave existing contents intact
                Logger.LogError(ex, "Failed to list the contents of a folder.");
                if (BrowserViewModel.TransferViewModel is { } transferViewModel)
                    await transferViewModel.ReportErrorAsync("FolderLoadFailed".ToLocalized());
            }
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

        private void ApplyAdaptiveLayout()
        {
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
                BrowserViewModel.Layouts.CurrentViewOption = SettingsService.UserSettings.AreThumbnailsEnabled
                    ? galleryView
                    : gridView;
            }
            else if (imagePercentage + mediaPercentage >= 70f)
            {
                // GridView
                BrowserViewModel.Layouts.CurrentViewOption = gridView;
            }
            else if (documentPercentage + folderPercentage >= 50f)
            {
                // GridView
                BrowserViewModel.Layouts.CurrentViewOption = gridView;
            }
            else if (otherPercentage + folderPercentage >= 50f)
            {
                // ColumnView
                BrowserViewModel.Layouts.CurrentViewOption = columnView;
            }
            else
            {
                // ListView
                BrowserViewModel.Layouts.CurrentViewOption = listView;
            }
        }
    }
}
