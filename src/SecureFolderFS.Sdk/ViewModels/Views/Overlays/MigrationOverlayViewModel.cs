using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    [Inject<IVaultManagerService>]
    public sealed partial class MigrationOverlayViewModel : OverlayViewModel, IProgress<double>, IProgress<IResult>, INotifyStateChanged, IAsyncInitialize, IDisposable
    {
        private IDisposable? _unlockContract;
        private IVaultMigratorModel? _vaultMigrator;

        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private double _CurrentProgress;
        [ObservableProperty] private MigrationViewModel _MigrationViewModel;

        /// <inheritdoc/>
        public event EventHandler<EventArgs>? StateChanged;

        public MigrationOverlayViewModel(MigrationViewModel migrationViewModel)
        {
            // For simplicity's sake there's no inheritance of MigrationViewModel,
            // and appropriate migrators are chosen based solely on vault version
            ServiceProvider = DI.Default;
            MigrationViewModel = migrationViewModel;
            PrimaryButtonText = "Continue".ToLocalized();
            Title = "Authenticate".ToLocalized();
            SecondaryButtonEnabled = true;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            _vaultMigrator = await VaultManagerService.GetMigratorAsync(MigrationViewModel.VaultFolder, cancellationToken);
        }

        /// <inheritdoc/>
        public void Report(double value)
        {
            CurrentProgress = value;
        }

        /// <inheritdoc/>
        public void Report(IResult value)
        {
            if (value.Successful)
                StateChanged?.Invoke(this, new MigrationCompletedEventArgs());
            else
                StateChanged?.Invoke(this, new ErrorReportedEventArgs(value));
        }

        [RelayCommand]
        private async Task AuthenticateMigrationAsync(object? credentials, CancellationToken cancellationToken)
        {
            if (credentials is null)
                return;

            if (_vaultMigrator is null)
                return;

            try
            {
                _unlockContract?.Dispose();
                _unlockContract = await _vaultMigrator.UnlockAsync(credentials, cancellationToken);

                StateChanged?.Invoke(this, new VaultUnlockedEventArgs(_unlockContract, MigrationViewModel.VaultFolder, false));
                Title = "Migrate".ToLocalized();
                PrimaryButtonText = null;
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
        }

        [RelayCommand]
        private async Task MigrateAsync(CancellationToken cancellationToken)
        {
            if (_unlockContract is null)
                return;

            if (_vaultMigrator is null)
                return;

            try
            {
                // Start operation and report initial progress
                Report(0);
                IsProgressing = true;
                SecondaryButtonEnabled = false;

                // Await a short delay for better UX
                #if !DEBUG
                #endif
                await Task.Delay(1000);

                // Migrate
                await _vaultMigrator.MigrateAsync(_unlockContract, new(this), cancellationToken);
                Title = "Summary".ToLocalized();
                Report(Result.Success);
            }
            catch (Exception ex)
            {
                Report(Result.Failure(ex));
            }
            finally
            {
                IsProgressing = false;
                SecondaryButtonEnabled = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _unlockContract?.Dispose();
        }
    }
}
