using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public partial class InfoBarViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _Title;

        [ObservableProperty]
        private string? _Message;

        [ObservableProperty]
        private bool _IsOpen;

        [ObservableProperty]
        private bool _CanBeClosed = true;

        [ObservableProperty]
        private InfoBarSeverityType _InfoBarSeverity;
    }
}
