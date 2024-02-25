using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    public class FileViewModel : BrowserItemViewModel
    {
        /// <inheritdoc/>
        public override IStorable Inner => File;

        /// <summary>
        /// Gets the file associated with this view model.
        /// </summary>
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
