using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class VaultItemInfoOverlayViewModel : OverlayViewModel, IAsyncInitialize
    {
        private readonly IStorable _ciphertextItem;
        private readonly IStorable _plaintextItem;

        [ObservableProperty] private string? _PlaintextSize;
        [ObservableProperty] private string? _PlaintextFullSize;
        [ObservableProperty] private string? _CiphertextSize;
        [ObservableProperty] private string? _CiphertextFullSize;
        [ObservableProperty] private string? _PlaintextName;
        [ObservableProperty] private string? _CiphertextName;
        [ObservableProperty] private string? _PlaintextPath;
        [ObservableProperty] private string? _CiphertextPath;
        [ObservableProperty] private bool _IsFile;

        public VaultItemInfoOverlayViewModel(IStorable ciphertextItem, IStorable plaintextItem)
        {
            IsFile = ciphertextItem is IFile;
            _ciphertextItem = ciphertextItem;
            _plaintextItem = plaintextItem;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            CiphertextPath = _ciphertextItem.Id;
            CiphertextName = _ciphertextItem.Name;
            PlaintextPath = _plaintextItem.Id;
            PlaintextName = _plaintextItem.Name;

            if (!IsFile)
                return;

            var ciphertextSize = await ((IFile)_ciphertextItem).GetSizeAsync(cancellationToken);
            var plaintextSize = await ((IFile)_plaintextItem).GetSizeAsync(cancellationToken);

            if (ciphertextSize.HasValue)
            {
                CiphertextSize = ByteSize.FromBytes(ciphertextSize.Value).ToString();
                CiphertextFullSize = $"{ciphertextSize.Value} B";
            }

            if (plaintextSize.HasValue)
            {
                PlaintextSize = ByteSize.FromBytes(plaintextSize.Value).ToString();
                PlaintextFullSize = $"{plaintextSize.Value} B";

                if (ciphertextSize.HasValue)
                {
                    var difference = ciphertextSize.Value - plaintextSize.Value;
                    CiphertextSize += $" (+{ByteSize.FromBytes(difference)})";
                }
            }
        }
    }
}