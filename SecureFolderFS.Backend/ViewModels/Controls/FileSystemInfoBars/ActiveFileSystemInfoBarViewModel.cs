using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Helpers;

namespace SecureFolderFS.Backend.ViewModels.Controls.FileSystemInfoBars
{
    public sealed class ActiveFileSystemInfoBarViewModel : ObservableObject
    {
        private DokanyInfoBarViewModel? _dokanyInfoBarViewModel;

        private InfoBarViewModel? _InfoBarViewModel;
        public InfoBarViewModel? InfoBarViewModel
        {
            get => _InfoBarViewModel;
            set => SetProperty(ref _InfoBarViewModel, value);
        }

        public void ConfigureFileSystem(FileSystemAdapterType adapter)
        {
            switch (adapter)
            {
                case FileSystemAdapterType.DokanAdapter:
                {
                    var result = FileSystemAvailabilityHelpers.IsDokanyAvailable();
                    if (result != FileSystemAvailabilityErrorType.FileSystemAvailable)
                    {
                        _dokanyInfoBarViewModel ??= new DokanyInfoBarViewModel();

                        InfoBarViewModel = _dokanyInfoBarViewModel;
                        InfoBarViewModel.IsOpen = true;
                        InfoBarViewModel.InfoBarSeverity = InfoBarSeverityType.Error;
                        InfoBarViewModel.CanBeClosed = false;

                        const string DOKANY_NOT_FOUND = "Dokany has not been detected. Please install Dokany to continue using SecureFolderFS.";
                        InfoBarViewModel.Message = result switch
                        {
                            FileSystemAvailabilityErrorType.ModuleNotAvailable => DOKANY_NOT_FOUND,
                            FileSystemAvailabilityErrorType.DriverNotAvailable => DOKANY_NOT_FOUND,
                            FileSystemAvailabilityErrorType.VersionTooLow => "The installed version of Dokany is outdated. Please update Dokany to match requested version.",
                            FileSystemAvailabilityErrorType.VersionTooHigh => "The installed version of Dokany is not compatible with SecureFolderFS version. Please install requested version of Dokany.",
                            _ => "SecureFolderFS cannot work with installed Dokany version. Please install requested version of Dokany."
                        };
                    }
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
