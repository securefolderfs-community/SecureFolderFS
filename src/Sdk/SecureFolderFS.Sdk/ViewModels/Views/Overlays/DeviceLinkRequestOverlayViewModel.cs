using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.PhoneLink.Models;
using SecureFolderFS.Sdk.PhoneLink.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class DeviceLinkRequestOverlayViewModel : OverlayViewModel, IStagingView
    {
        private readonly DeviceLinkService _deviceLinkService;

        [ObservableProperty] private string? _RemoteDeviceName;
        [ObservableProperty] private string? _CredentialName;

        public DeviceLinkRequestOverlayViewModel(DeviceLinkService deviceLinkService, AuthenticationRequestModel model)
        {
            _deviceLinkService = deviceLinkService;
            RemoteDeviceName = model.DesktopName;
            CredentialName = "CredentialUsed".ToLocalized(model.CredentialName);
        }

        [RelayCommand]
        public Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            _deviceLinkService.ConfirmAuthentication(true);
            return Task.FromResult<IResult>(Result.Success);
        }

        [RelayCommand]
        public Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            _deviceLinkService.ConfirmAuthentication(false);
            return Task.FromResult<IResult>(Result.Success);
        }
    }
}