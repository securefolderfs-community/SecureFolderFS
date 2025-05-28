using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public sealed partial class TextPreviewerViewModel : FilePreviewerViewModel
    {
        [ObservableProperty] private string? _Text;

        public TextPreviewerViewModel(IFile file)
            : base(file)
        {
            Title = file.Name;
            IsToolbarOnTop = true;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Text = await Inner.ReadAllTextAsync(Encoding.UTF8, cancellationToken);
        }
    }
}
