using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Inject<IOverlayService>, Inject<IVaultService>]
    [Bindable(true)]
    public partial class MigrationViewModel : ReportableViewModel
    {
        private readonly IVaultModel _vaultModel;

        [ObservableProperty] private string? _CurrentVersion;
        [ObservableProperty] private string? _NewVersion;

        /// <summary>
        /// Gets the current vault version.
        /// </summary>
        public int FormatVersion { get; }

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public MigrationViewModel(IVaultModel vaultModel, int currentVersion)
        {
            ServiceProvider = DI.Default;
            _vaultModel = vaultModel;
            _CurrentVersion = $"Version {currentVersion}";
            _NewVersion = $"Version {VaultService.LatestVaultVersion}";
            FormatVersion = currentVersion;
        }

        /// <inheritdoc/>
        public override void SetError(IResult? result)
        {
            _ = result;
        }

        [RelayCommand]
        private async Task OpenMigrationOverlayAsync()
        {
            var result = await OverlayService.ShowAsync(new MigrationOverlayViewModel(this));
            if (result.Positive())
                StateChanged?.Invoke(this, new MigrationCompletedEventArgs());
        }
    }
}
