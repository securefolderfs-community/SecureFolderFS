using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.PhoneLink.Models;
using SecureFolderFS.Sdk.PhoneLink.Services;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IMediaService>]
    public sealed partial class DeviceLinkRequestOverlayViewModel : OverlayViewModel, IAsyncInitialize, IStagingView
    {
        private readonly DeviceLinkService _deviceLinkService;
        private readonly AuthenticationRequestModel _requestModel;

        [ObservableProperty] private IImage? _DesktopImage;
        [ObservableProperty] private string? _CredentialName;
        [ObservableProperty] private string? _RemoteDeviceName;

        public DeviceLinkRequestOverlayViewModel(DeviceLinkService deviceLinkService, AuthenticationRequestModel requestModel)
        {
            ServiceProvider = DI.Default;
            _deviceLinkService = deviceLinkService;
            _requestModel = requestModel;
            RemoteDeviceName = requestModel.DesktopName;
            CredentialName = "CredentialUsed".ToLocalized(requestModel.CredentialName);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            DesktopImage = await MediaService.GetImageFromResourceAsync($"{_requestModel.DesktopType}_Device", cancellationToken);
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