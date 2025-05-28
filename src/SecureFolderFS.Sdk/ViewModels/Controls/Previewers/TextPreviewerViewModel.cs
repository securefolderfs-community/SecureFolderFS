using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Previewers
{
    [Bindable(true)]
    public sealed partial class TextPreviewerViewModel : FilePreviewerViewModel, IPersistable
    {
        [ObservableProperty] private string? _Text;
        [ObservableProperty] private bool _IsReadOnly;
        [ObservableProperty] private bool _IsWrappingText;

        public TextPreviewerViewModel(IFile file, bool isReadOnly)
            : base(file)
        {
            Title = file.Name;
            IsReadOnly = isReadOnly;
            IsToolbarOnTop = true;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Text = await Inner.ReadAllTextAsync(Encoding.UTF8, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            if (IsReadOnly)
                return;

            if (Text is not null)
                await Inner.WriteAllTextAsync(Text, null, cancellationToken);
        }
    }
}
