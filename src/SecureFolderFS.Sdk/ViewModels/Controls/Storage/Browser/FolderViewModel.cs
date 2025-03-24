using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Storage.Browser
{
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
            Items.Clear();

            var items = await Folder.GetItemsAsync(StorableType.All, cancellationToken).ToArrayAsync(cancellationToken: cancellationToken);
            BrowserViewModel.ViewOptions.GetSorter()?.SortCollection(items.Select(x => (BrowserItemViewModel)(x switch
            {
                IFile file => new FileViewModel(file, BrowserViewModel, this).WithInitAsync(cancellationToken),
                IFolder folder => new FolderViewModel(folder, BrowserViewModel, this).WithInitAsync(cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(x))
            })), Items);
        }

        /// <inheritdoc/>
        public virtual void OnAppearing()
        {
        }

        /// <inheritdoc/>
        public virtual void OnDisappearing()
        {
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
    }
}
