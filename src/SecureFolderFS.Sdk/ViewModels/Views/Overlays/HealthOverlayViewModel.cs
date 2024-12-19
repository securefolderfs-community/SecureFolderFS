using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class HealthOverlayViewModel : BaseDesignationViewModel, IUnlockedViewContext, IDisposable
    {
        private readonly SynchronizationContext? _context;

        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private SeverityType _Severity;
        [ObservableProperty] private ObservableCollection<HealthIssueViewModel> _FoundIssues;

        /// <inheritdoc/>
        public UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        /// <inheritdoc/>
        public VaultViewModel VaultViewModel => UnlockedVaultViewModel.VaultViewModel;

        public HealthOverlayViewModel(UnlockedVaultViewModel unlockedVaultViewModel, SynchronizationContext? context)
        {
            _context = context;
            UnlockedVaultViewModel = unlockedVaultViewModel;
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

            if (FoundIssues.IsEmpty())
            {
                _context?.Post(_ => Severity = SeverityType.Success, null);
                return;
            }

            var enumerable = e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null } ? e.NewItems : FoundIssues;
            UpdateSeverity(enumerable);
        }

        private void UpdateSeverity(IEnumerable enumerable)
        {
            if (FoundIssues.IsEmpty())
            {
                _context?.Post(_ => Severity = SeverityType.Success, null);
                return;
            }

            var newSeverity = Severity;
            foreach (HealthIssueViewModel item in enumerable)
            {
                if (newSeverity < item.Severity)
                    newSeverity = item.Severity;
            }

            _Severity = newSeverity;
            _context?.Post(_ => OnPropertyChanged(nameof(Severity)), null);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            FoundIssues.CollectionChanged -= FoundIssues_CollectionChanged;
        }
    }
}
