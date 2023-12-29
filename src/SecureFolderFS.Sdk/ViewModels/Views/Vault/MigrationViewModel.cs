using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    public sealed partial class MigrationViewModel : ReportableViewModel
    {
        [ObservableProperty] private string? _CurrentVersion;
        [ObservableProperty] private string? _NewVersion;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public MigrationViewModel(int newVersion)
        {
            _NewVersion = $"Update — Version {newVersion}";
        }

        public MigrationViewModel(int currentVersion, int newVersion)
        {
            _CurrentVersion = $"Version {currentVersion}";
            _NewVersion = $"Version {newVersion}";
        }

        /// <inheritdoc/>
        public override void SetError(IResult? result)
        {
            _ = result;
        }

        [RelayCommand]
        private async Task MigrateAsync()
        {
            // TODO
        }
    }
}
