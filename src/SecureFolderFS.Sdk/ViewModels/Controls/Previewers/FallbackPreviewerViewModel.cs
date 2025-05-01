using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public sealed class FallbackPreviewerViewModel : BasePreviewerViewModel<string>
    {
        private readonly IFile? _file;

        public FallbackPreviewerViewModel(IFile file)
        {
            _file = file;
            Title = _file.Name;
            Source = _file.Name;
        }

        public FallbackPreviewerViewModel(string itemName)
        {
            Title = itemName;
            Source = itemName;
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}