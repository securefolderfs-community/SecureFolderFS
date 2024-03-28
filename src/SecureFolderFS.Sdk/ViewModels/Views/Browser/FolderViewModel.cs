using OwlCore.Storage;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    public class FolderViewModel : BrowserItemViewModel
    {
        /// <summary>
        /// Gets the folder associated with this view model.
        /// </summary>
        public IFolder Folder { get; }

        /// <summary>
        /// Gets the items in this folder.
        /// </summary>
        public ObservableCollection<BrowserItemViewModel> Items { get; }

        /// <inheritdoc/>
        public override IStorable Inner => Folder;

        public FolderViewModel(IFolder folder)
        {
            Folder = folder;
            Title = folder.Name;
            Items = new();
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in Folder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                Items.Add(item switch
                {
                    IFile file => new FileViewModel(file),
                    IFolder folder => new FolderViewModel(folder),
                    _ => throw new ArgumentOutOfRangeException(nameof(item))
                });
            }

            // TODO: Load thumbnail
        }
    }
}
