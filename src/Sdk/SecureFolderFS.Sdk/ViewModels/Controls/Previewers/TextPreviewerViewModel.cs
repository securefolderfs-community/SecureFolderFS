using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        [ObservableProperty] private long _CharacterCount;
        [ObservableProperty] private int _CursorPosition;
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
            try
            {
                _persistedText = await Inner.ReadAllTextAsync(Encoding.UTF8, cancellationToken);
                Text = _persistedText;
            }
            finally
            {
                IsProgressing = false;
            }
        }

        /// <inheritdoc/>
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            if (IsReadOnly)
                return;

            if (Text is null)
                return;

            await Inner.WriteTextAsync(Text, cancellationToken);
            _persistedText = Text;
            WasModified = false;
        }

        [RelayCommand]
        private async Task SaveDocumentAsync(CancellationToken cancellationToken)
        {
            if (IsProgressing)
                return;

            IsProgressing = true;
            try
            {
                await SaveAsync(cancellationToken);
            }
            catch (Exception)
            {
                // WasModified stays true, so the modified indicator keeps signaling the unsaved state
            }
            finally
            {
                IsProgressing = false;
            }
        }

        partial void OnTextChanged(string? value)
        {
            // Compare lengths first to avoid a full string comparison on every keystroke
            WasModified = value?.Length != _persistedText?.Length || value != _persistedText;
            CharacterCount = value?.Length ?? 0L;
        }
    }
}
