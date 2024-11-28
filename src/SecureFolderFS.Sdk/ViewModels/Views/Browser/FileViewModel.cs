using OwlCore.Storage;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Browser
{
    [Bindable(true)]
    public class FileViewModel : BrowserItemViewModel
    {
        /// <inheritdoc/>
        public override IStorable Inner => File;

        /// <summary>
        /// Gets the file associated with this view model.
        /// </summary>
        public IFile File { get; }

        public FileViewModel(IFile file, FolderViewModel? parentFolder)
            : base(parentFolder)
        {
            File = file;
            Title = file.Name;
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Load thumbnail
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task OpenAsync(CancellationToken cancellationToken)
        {
            // TODO: Open file
            return Task.CompletedTask;
        }
    }
}
