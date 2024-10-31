using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Bindable(true)]
    public sealed partial class HealthOverlayViewModel : OverlayViewModel, IDisposable
    {
        private readonly SynchronizationContext? _context;

        [ObservableProperty] private SeverityType _Severity;

        public ObservableCollection<HealthIssueViewModel> FoundIssues { get; }

        public HealthOverlayViewModel(SynchronizationContext? context)
        {
            _context = context;
            FoundIssues = new();
            FoundIssues.CollectionChanged += FoundIssues_CollectionChanged;
        }

        private void FoundIssues_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (FoundIssues.IsEmpty())
            {
                _context?.Post(_ => Severity = SeverityType.Success, null);
                return;
            }

            var newSeverity = Severity;
            var enumerable = e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null } ? e.NewItems : FoundIssues;

            foreach (HealthIssueViewModel item in enumerable)
            {
                if (newSeverity < item.Severity)
                    newSeverity = item.Severity;
            }

            _context?.Post(_ => Severity = newSeverity, null);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            FoundIssues.CollectionChanged -= FoundIssues_CollectionChanged;
        }
    }
}
