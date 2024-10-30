using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    // TODO: add docs
    [Bindable(true)]
    public partial class InfoBarViewModel : ObservableObject, IViewable
    {
        [ObservableProperty] private bool _IsOpen;
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private string? _Message;
        [ObservableProperty] private bool _IsCloseable;
        [ObservableProperty] private SeverityType _Severity;
    }
}
