using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public class InfoBarViewModel : ObservableObject
    {
        private string? _Title;
        public string? Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private string? _Message;
        public string? Message
        {
            get => _Message;
            set => SetProperty(ref _Message, value);
        }
        
        private bool _IsOpen;
        public bool IsOpen
        {
            get => _IsOpen;
            set => SetProperty(ref _IsOpen, value);
        }

        private bool _CanBeClosed = true;
        public bool CanBeClosed
        {
            get => _CanBeClosed;
            set => SetProperty(ref _CanBeClosed, value);
        }

        private InfoBarSeverityType _InfoBarSeverity = InfoBarSeverityType.Information;
        public InfoBarSeverityType InfoBarSeverity
        {
            get => _InfoBarSeverity;
            set => SetProperty(ref _InfoBarSeverity, value);
        }
    }
}
