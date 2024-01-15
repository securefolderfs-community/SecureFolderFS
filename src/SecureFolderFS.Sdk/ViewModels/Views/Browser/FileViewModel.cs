using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    public sealed class FileViewModel : StorageItemViewModel
    {
        public IFile File { get; }

        public FileViewModel(IFile file)
        {
            File = file;
            Title = file.Name;
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            // TODO: Load thumbnail
        }
    }
}
