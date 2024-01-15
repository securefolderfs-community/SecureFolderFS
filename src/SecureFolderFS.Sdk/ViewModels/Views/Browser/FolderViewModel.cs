using System;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    public sealed class FolderViewModel : StorageItemViewModel
    {
        public IFolder Folder { get; }

        public ObservableCollection<StorageItemViewModel> Items { get; }

        public FolderViewModel(IFolder folder)
        {
            Folder = folder;
            Title = folder.Name;
            Items = new();
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await foreach (var item in Folder.GetItemsAsync(StorableKind.All, cancellationToken))
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
