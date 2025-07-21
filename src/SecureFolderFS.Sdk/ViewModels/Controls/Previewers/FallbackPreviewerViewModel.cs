using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public sealed class FallbackPreviewerViewModel : FilePreviewerViewModel
    {
        public FallbackPreviewerViewModel(IFile file)
            : base(file)
        {
            Title = file.Name;
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}