using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        /// Gets the <see cref="Views.Vault.BrowserViewModel"/> instance, which this folder belongs to.
        /// </summary>
        public BrowserViewModel BrowserViewModel { get; }
        
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
            : base(parentFolder)
        {
            Folder = folder;
            BrowserViewModel = browserViewModel;
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
            await foreach (var item in Folder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                Items.Add(item switch
                {
                    IFile file => new FileViewModel(file, this).WithInitAsync(),
                    IFolder folder => new FolderViewModel(folder, BrowserViewModel, this).WithInitAsync(),
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
        protected override void UpdateStorable(IStorable storable)
        {
            Folder = (IFolder)storable;
        }

        /// <inheritdoc/>
        protected override async Task OpenAsync(CancellationToken cancellationToken)
        {
            if (Items.IsEmpty())
                _ = ListContentsAsync(cancellationToken);
            
            await BrowserViewModel.Navigator.NavigateAsync(this);
        }
    }
}
