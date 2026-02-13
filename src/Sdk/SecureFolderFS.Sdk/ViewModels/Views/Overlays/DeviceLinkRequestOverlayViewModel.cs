using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.DeviceLink.Models;
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
        private readonly AuthenticationRequestViewModel _requestViewModel;

        [ObservableProperty] private IImage? _DesktopImage;
        [ObservableProperty] private string? _CredentialName;
        [ObservableProperty] private string? _RemoteDeviceName;

        public DeviceLinkRequestOverlayViewModel(AuthenticationRequestViewModel requestViewModel)
        {
            ServiceProvider = DI.Default;
            _requestViewModel = requestViewModel;
            RemoteDeviceName = requestViewModel.DesktopName;
            CredentialName = "CredentialUsed".ToLocalized(requestViewModel.CredentialName);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            DesktopImage = await MediaService.GetImageFromResourceAsync($"{_requestViewModel.DesktopType}_Device", cancellationToken);
        }

        [RelayCommand]
        public Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            _requestViewModel.Confirm(true);
            return Task.FromResult<IResult>(Result.Success);
        }

        [RelayCommand]
        public Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            _requestViewModel.Confirm(false);
            return Task.FromResult<IResult>(Result.Success);
        }
    }
}