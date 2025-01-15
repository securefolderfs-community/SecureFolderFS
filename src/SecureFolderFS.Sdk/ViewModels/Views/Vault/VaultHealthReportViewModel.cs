using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Bindable(true)]
    [Inject<IVaultFileSystemService>]
    public sealed partial class VaultHealthReportViewModel : BaseDesignationViewModel, IUnlockedViewContext, IDisposable
    {
        private readonly SynchronizationContext? _context;

        [ObservableProperty] private bool _CanResolve;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private double _CurrentProgress;
        [ObservableProperty] private SeverityType _Severity;
        [ObservableProperty] private ObservableCollection<HealthIssueViewModel> _FoundIssues;

        /// <inheritdoc/>
        public UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        /// <inheritdoc/>
        public VaultViewModel VaultViewModel => UnlockedVaultViewModel.VaultViewModel;

        public VaultHealthReportViewModel(UnlockedVaultViewModel unlockedVaultViewModel, SynchronizationContext? context)
        {
            ServiceProvider = DI.Default;
            UnlockedVaultViewModel = unlockedVaultViewModel;
            Title = "HealthReport".ToLocalized();
            FoundIssues = new();
            FoundIssues.CollectionChanged += FoundIssues_CollectionChanged;
            _context = context;
        }

        partial void OnIsProgressingChanged(bool value)
        {
            _ = value;
            UpdateSeverity(FoundIssues);
        }

        [RelayCommand]
        private async Task ResolveAsync(CancellationToken cancellationToken)
        {
            UnlockedVaultViewModel.Options.DangerousSetReadOnly(true);
            await VaultFileSystemService.ResolveIssuesAsync(FoundIssues, UnlockedVaultViewModel.StorageRoot, IssueResolved, cancellationToken);
            UnlockedVaultViewModel.Options.DangerousSetReadOnly(false);
        }

        private void IssueResolved(HealthIssueViewModel issueViewModel, IResult result)
        {
            if (result.Successful)
                FoundIssues.RemoveMatch(x => x.Inner.Id == issueViewModel.Inner.Id);
        }

        private void FoundIssues_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsProgressing)
                return;

            UpdateSeverity(e is
            {
                Action:
                    NotifyCollectionChangedAction.Add or
                    NotifyCollectionChangedAction.Replace or
                    NotifyCollectionChangedAction.Move,
                NewItems: not null
            } ? e.NewItems : FoundIssues);
        }

        private void UpdateSeverity(IEnumerable enumerable)
        {
#pragma warning disable MVVMTK0034
            var severity = Severity;
            if (severity != SeverityType.Success && FoundIssues.IsEmpty())
            {
                _Severity = SeverityType.Success;
                _context?.Post(_ => OnPropertyChanged(nameof(Severity)), null);
                return;
            }

            foreach (HealthIssueViewModel item in enumerable)
            {
                if (severity < item.Severity)
                    severity = item.Severity;
            }

            if (Severity != severity)
            {
                _Severity = severity;
                _context?.Post(_ => OnPropertyChanged(nameof(Severity)), null);
            }
#pragma warning restore MVVMTK0034
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            FoundIssues.CollectionChanged -= FoundIssues_CollectionChanged;
        }
    }
}
