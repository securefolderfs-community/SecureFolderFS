using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls.Authentication
{
    [Inject<IOverlayService>]
    [Bindable(true)]
    public sealed partial class MigrationViewModel : ReportableViewModel
    {
        [ObservableProperty] private string? _CurrentVersion;
        [ObservableProperty] private string? _NewVersion;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public MigrationViewModel(int newVersion)
        {
            ServiceProvider = DI.Default;
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
            var result = await OverlayService.ShowAsync(new MigrationOverlayViewModel());
            if (result.Positive())
                StateChanged?.Invoke(this, new MigrationCompletedEventArgs());
        }
    }
}
