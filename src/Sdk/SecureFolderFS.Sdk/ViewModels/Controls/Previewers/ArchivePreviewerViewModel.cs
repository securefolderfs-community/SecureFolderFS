using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public sealed partial class ArchivePreviewerViewModel : FilePreviewerViewModel
    {
        public ArchivePreviewerViewModel(IFile file)
            : base(file)
        {
            Title = file.Name;
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}