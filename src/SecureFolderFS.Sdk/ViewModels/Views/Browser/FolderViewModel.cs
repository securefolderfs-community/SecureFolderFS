using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.ViewModels.Controls.Transfer;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    [Bindable(true)]
    public partial class FolderViewModel : BrowserItemViewModel, IViewDesignation
    {
        public INavigator Navigator { get; }
        
        public TransferViewModel? TransferViewModel { get; }
        
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

        public FolderViewModel(IFolder folder, FolderViewModel parentFolder, TransferViewModel? transferViewModel)
            : this(folder, parentFolder.Navigator, transferViewModel, parentFolder)
        {
        }

        public FolderViewModel(IFolder folder, INavigator navigator, TransferViewModel? transferViewModel, FolderViewModel? parentFolder)
            : base(parentFolder)
        {
            Folder = folder;
            Navigator = navigator;
            Title = folder.Name;
            TransferViewModel = transferViewModel;
            Items = new();
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Load thumbnail
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override void UpdateStorable(IStorable storable)
        {
            Folder = (IFolder)storable;
        }

        public async Task ListContentsAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in Folder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                Items.Add(item switch
                {
                    IFile file => new FileViewModel(file, this).WithInitAsync(),
                    IFolder folder => new FolderViewModel(folder, this, TransferViewModel).WithInitAsync(),
                    _ => throw new ArgumentOutOfRangeException(nameof(item))
                });
            }
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
        protected override async Task OpenAsync(CancellationToken cancellationToken)
        {
            if (Items.IsEmpty())
                _ = ListContentsAsync(cancellationToken);
            
            await Navigator.NavigateAsync(this);
        }
    }
}
