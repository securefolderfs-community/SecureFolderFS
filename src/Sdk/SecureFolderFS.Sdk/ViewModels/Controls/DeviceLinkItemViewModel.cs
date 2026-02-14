using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.DeviceLink.ViewModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Bindable(true)]
    [Inject<IMediaService>]
    public sealed partial class DeviceLinkItemViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty] private IImage? _Icon;
        [ObservableProperty] private CredentialViewModel _CredentialViewModel;

        public DeviceLinkItemViewModel(CredentialViewModel credentialViewModel)
        {
            ServiceProvider = DI.Default;
            CredentialViewModel = credentialViewModel;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            Icon = await MediaService.GetImageFromResourceAsync($"{CredentialViewModel.MachineType}_Device", cancellationToken);
        }
    }
}
