using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class VaultItemInfoOverlayViewModel : OverlayViewModel
    {
        [ObservableProperty] private string? _PlaintextName;
        [ObservableProperty] private string? _CiphertextName;
        [ObservableProperty] private string? _PlaintextPath;
        [ObservableProperty] private string? _CiphertextPath;
        [ObservableProperty] private bool _IsFile;

        public VaultItemInfoOverlayViewModel(IStorable ciphertextItem, string plaintextPath)
        {
            IsFile = ciphertextItem is IFile;
            CiphertextPath = ciphertextItem.Id;
            CiphertextName = ciphertextItem.Name;
            PlaintextPath = plaintextPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            PlaintextName = Path.GetFileName(plaintextPath);
        }
    }
}