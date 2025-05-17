using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public sealed class TextPreviewerViewModel : FilePreviewerViewModel<string>
    {
        public TextPreviewerViewModel(IFile file)
            : base(file)
        {
            Title = file.Name;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Source = await Inner.ReadAllTextAsync(Encoding.UTF8, cancellationToken);
        }
    }
}
