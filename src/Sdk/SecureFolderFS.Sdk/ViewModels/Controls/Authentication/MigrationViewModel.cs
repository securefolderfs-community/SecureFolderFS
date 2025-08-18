using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Inject<IOverlayService>, Inject<IVaultService>]
    [Bindable(true)]
    public partial class MigrationViewModel : ReportableViewModel
    {
        [ObservableProperty] private string? _CurrentVersion;
        [ObservableProperty] private string? _NewVersion;

        /// <summary>
        /// Gets the current vault version.
        /// </summary>
        public int FormatVersion { get; }

        /// <summary>
        /// Gets the <see cref="IFolder"/> instance associated with the vault for migration.
        /// </summary>
        public IFolder VaultFolder { get; }

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public MigrationViewModel(IFolder vaultFolder, int currentVersion)
        {
            ServiceProvider = DI.Default;
            CurrentVersion = $"Version {currentVersion}";
            NewVersion = $"Version {VaultService.LatestVaultVersion}";
            VaultFolder = vaultFolder;
            FormatVersion = currentVersion;
        }

        /// <inheritdoc/>
        public override void Report(IResult? result)
        {
            _ = result;
        }

        [RelayCommand]
        private async Task OpenMigrationOverlayAsync(CancellationToken cancellationToken)
        {
            using var migrationOverlay = new MigrationOverlayViewModel(this);
            await migrationOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(migrationOverlay);

            // Notify state changed after successful or unsuccessful migration
            StateChanged?.Invoke(this, new MigrationCompletedEventArgs());
        }
    }
}
