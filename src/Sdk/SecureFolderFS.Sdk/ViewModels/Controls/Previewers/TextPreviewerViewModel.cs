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
    public sealed partial class TextPreviewerViewModel : FilePreviewerViewModel, IChangeTracker, IPersistable
    {
        private string? _persistedText;

        [ObservableProperty] private string? _Text;
        [ObservableProperty] private bool _IsReadOnly;
        [ObservableProperty] private bool _WasModified;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private bool _IsWrappingText;

        public TextPreviewerViewModel(IFile file, bool isReadOnly)
            : base(file)
        {
            Title = file.Name;
            IsReadOnly = isReadOnly;
            IsToolbarOnTop = true;
            IsWrappingText = true;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            IsProgressing = true;
            await Task.Delay(100);

            _persistedText = await Inner.ReadAllTextAsync(Encoding.UTF8, cancellationToken);
            Text = _persistedText;
            IsProgressing = false;
        }

        /// <inheritdoc/>
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            if (IsReadOnly)
                return;

            if (Text is null)
                return;

            //await Task.Delay(5000, cancellationToken);
            await Inner.WriteTextAsync(Text, cancellationToken);
            _persistedText = Text;
            WasModified = false;
        }

        partial void OnTextChanged(string? value)
        {
            WasModified = value != _persistedText;
        }
    }
}
