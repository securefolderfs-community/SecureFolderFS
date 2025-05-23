using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser
{
    [Inject<ISettingsService>]
    [Bindable(true)]
    public partial class FolderViewModel : BrowserItemViewModel, IViewDesignation
    {
        /// <summary>
        /// Gets the folder associated with this view model.
        /// </summary>
        public IFolder Folder { get; protected set; }

        /// <summary>
        /// Gets the items in this folder.
        /// </summary>
        public ObservableCollection<BrowserItemViewModel> Items { get; }

        /// <inheritdoc/>
        public override IStorable Inner => Folder;

        public FolderViewModel(IFolder folder, BrowserViewModel browserViewModel, FolderViewModel? parentFolder)
            : base(browserViewModel, parentFolder)
        {
            ServiceProvider = DI.Default;
            Folder = folder;
            Title = folder.Name;
            Items = new();
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Load thumbnail
            return Task.CompletedTask;
        }

        public async Task ListContentsAsync(CancellationToken cancellationToken = default)
        {
            Items.DisposeElements();
            Items.Clear();

            var items = await Folder.GetItemsAsync(StorableType.All, cancellationToken).ToArrayAsync(cancellationToken: cancellationToken);
            BrowserViewModel.Layouts.GetSorter().SortCollection(items.Select(x => (BrowserItemViewModel)(x switch
            {
                IFile file => new FileViewModel(file, BrowserViewModel, this),
                IFolder folder => new FolderViewModel(folder, BrowserViewModel, this),
                _ => throw new ArgumentOutOfRangeException(nameof(x))
            })), Items);

            // Apply adaptive layout
            if (SettingsService.UserSettings.IsAdaptiveLayoutEnabled)
                ApplyAdaptiveLayout();
        }

        /// <inheritdoc/>
        public virtual void OnAppearing()
        {
            // Apply adaptive layout when back to a folder
            if (!Items.IsEmpty() && SettingsService.UserSettings.IsAdaptiveLayoutEnabled)
                ApplyAdaptiveLayout();
        }

        /// <inheritdoc/>
        public virtual void OnDisappearing()
        {
            Items.DisposeElements();
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
            var folderPercentage = 100f * Items.Count(IsFolder) / itemCount;
            var imagePercentage = 100f * Items.Count(IsImage) / itemCount;
            var mediaPercentage = 100f * Items.Count(IsMedia) / itemCount;
            var documentPercentage = 100f * Items.Count(IsDocument) / itemCount;
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

            static bool IsFolder(BrowserItemViewModel item)
                => item.Inner is IFolder;

            static bool IsImage(BrowserItemViewModel item)
                => FileTypeHelper.GetClassification(item.Inner) is { TypeHint: TypeHint.Image };

            static bool IsMedia(BrowserItemViewModel item)
                => FileTypeHelper.GetClassification(item.Inner) is { TypeHint: TypeHint.Media };

            static bool IsDocument(BrowserItemViewModel item)
                => FileTypeHelper.GetClassification(item.Inner) is { TypeHint: TypeHint.Document or TypeHint.Plaintext };
        }
    }
}
