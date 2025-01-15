using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public sealed class TextPreviewerViewModel : BasePreviewerViewModel<string>
    {
        private readonly IFile? _file;

        public TextPreviewerViewModel(IFile file)
        {
            _file = file;
            Title = file.Name;
        }

        public TextPreviewerViewModel(string text)
        {
            Source = text;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (_file is not null)
                Source = await _file.ReadAllTextAsync(Encoding.UTF8, cancellationToken);
        }
    }
}
