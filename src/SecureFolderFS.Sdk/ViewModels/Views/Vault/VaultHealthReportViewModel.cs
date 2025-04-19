using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Bindable(true)]
    [Inject<IVaultHealthService>]
    public sealed partial class VaultHealthReportViewModel : BaseDesignationViewModel, IDisposable
    {
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;
        
        [ObservableProperty] private bool _CanResolve;
        [ObservableProperty] private VaultHealthViewModel _HealthViewModel;
        
        public VaultHealthReportViewModel(UnlockedVaultViewModel unlockedVaultViewModel, VaultHealthViewModel healthViewModel)
        {
            ServiceProvider = DI.Default;
            HealthViewModel = healthViewModel;
            Title = "HealthReport".ToLocalized();
            HealthViewModel.StateChanged += HealthViewModel_StateChanged;
            _unlockedVaultViewModel = unlockedVaultViewModel;
        }

        [RelayCommand]
        private async Task ResolveAsync(CancellationToken cancellationToken)
        {
            // Get the readonly status and set IsReadOnly to true
            var isReadOnly = _unlockedVaultViewModel.Options.IsReadOnly;
            _unlockedVaultViewModel.Options.DangerousSetReadOnly(true);

            // Resolve issues and restore readonly status
            await VaultHealthService.ResolveIssuesAsync(HealthViewModel.FoundIssues, _unlockedVaultViewModel.StorageRoot, IssueResolved, cancellationToken);
            _unlockedVaultViewModel.Options.DangerousSetReadOnly(isReadOnly);
        }

        private void IssueResolved(HealthIssueViewModel issueViewModel, IResult result)
        {
            if (result.Successful)
                HealthViewModel.FoundIssues.RemoveMatch(x => x.Inner.Id == issueViewModel.Inner.Id);
        }
        
        private void HealthViewModel_StateChanged(object? sender, EventArgs e)
        {
            CanResolve = e switch
            {
                ScanningStartedEventArgs => false,
                ScanningFinishedEventArgs args => !args.WasCanceled && !HealthViewModel.FoundIssues.IsEmpty(),
                _ => CanResolve
            };
        }
        
        /// <inheritdoc/>
        public void Dispose()
        {
            HealthViewModel.StateChanged -= HealthViewModel_StateChanged;
            HealthViewModel.Dispose();
        }
    }
}
