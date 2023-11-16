using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    // TODO: add docs
    public partial class InfoBarViewModel : ObservableObject
    {
        [ObservableProperty] private bool _IsOpen;
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private string? _Message;
        [ObservableProperty] private bool _CanBeClosed;
        [ObservableProperty] private InfoBarSeverityType _Severity;
    }
}
