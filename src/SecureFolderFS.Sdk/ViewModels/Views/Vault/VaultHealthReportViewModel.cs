using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Bindable(true)]
    public sealed partial class VaultHealthReportViewModel : BaseDesignationViewModel, IUnlockedViewContext, IDisposable
    {
        private readonly SynchronizationContext? _context;

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
            _context = context;
            UnlockedVaultViewModel = unlockedVaultViewModel;
            Title = "HealthReport".ToLocalized();
            _FoundIssues = new();
            _FoundIssues.CollectionChanged += FoundIssues_CollectionChanged;
        }

        partial void OnIsProgressingChanged(bool value)
        {
            _ = value;
            UpdateSeverity(FoundIssues);
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
            var severity = Severity;
            if (severity != SeverityType.Success && FoundIssues.IsEmpty())
            {
                _context?.Post(_ => Severity = SeverityType.Success, null);
                return;
            }

            foreach (HealthIssueViewModel item in enumerable)
            {
                if (severity < item.Severity)
                    severity = item.Severity;
            }

            if (Severity != severity)
                _context?.Post(_ => Severity = severity, null);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            FoundIssues.CollectionChanged -= FoundIssues_CollectionChanged;
        }
    }
}
