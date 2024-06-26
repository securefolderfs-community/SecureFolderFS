using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class CredentialsViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly IFolder _vaultFolder;

        [ObservableProperty] private AuthenticationViewModel? _CurrentViewModel;
        [ObservableProperty] private AuthenticationViewModel? _OriginalViewModel;
        [ObservableProperty] private ObservableCollection<AuthenticationViewModel> _AuthenticationOptions;

        public CredentialsViewModel(IFolder vaultFolder)
        {
            ServiceProvider = Ioc.Default;
            AuthenticationOptions = new();
            _vaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Get authentication options
            var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            await foreach (var item in VaultService.GetCreationAsync(_vaultFolder, vaultOptions.VaultId!, cancellationToken))
            {
                AuthenticationOptions.Add(item);
                if (item.Id == vaultOptions.AuthenticationMethod[0]) // TODO: Based on <something> decide which index to use. In this case index 0 is equivalent to first-stage auth
                    OriginalViewModel = item;
            }

            // Set default authentication option
            CurrentViewModel = AuthenticationOptions.FirstOrDefault();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CurrentViewModel?.Dispose();
            AuthenticationOptions.DisposeElements();
        }
    }
}
