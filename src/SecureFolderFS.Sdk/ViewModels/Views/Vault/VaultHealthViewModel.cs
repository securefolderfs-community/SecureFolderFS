using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Health;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Scanners;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Bindable(true)]
    [Inject<IVaultHealthService>, Inject<IVaultService>]
    public sealed partial class VaultHealthViewModel : ObservableObject, IProgress<double>, IProgress<TotalProgress>, INotifyStateChanged, IViewable, IAsyncInitialize, IDisposable
    {
        private CancellationTokenSource? _cts;
        private IHealthModel? _healthModel;
        private string? _lastScanMode;
        private readonly SynchronizationContext? _context;
        private readonly List<HealthIssueViewModel> _savedState;
        private readonly UnlockedVaultViewModel _unlockedVaultViewModel;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private string? _Subtitle;
        [ObservableProperty] private string? _StatusTitle;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private double _CurrentProgress;
        [ObservableProperty] private Severity _Severity;
        [ObservableProperty] private ObservableCollection<HealthIssueViewModel> _FoundIssues;

        /// <inheritdoc/>
        public event EventHandler<EventArgs>? StateChanged;

        public VaultHealthViewModel(UnlockedVaultViewModel unlockedVaultViewModel)
        {
            ServiceProvider = DI.Default;
            _cts = new();
            _context = SynchronizationContext.Current;
            _savedState = new();
            _unlockedVaultViewModel = unlockedVaultViewModel;
            Title = "HealthNoProblems".ToLocalized();
            Subtitle = null;
            StatusTitle = "Perform integrity check";
            FoundIssues = new();
            FoundIssues.CollectionChanged += FoundIssues_CollectionChanged;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var contentFolder = await VaultHelpers.GetContentFolderAsync(_unlockedVaultViewModel.VaultFolder, cancellationToken);
            var folderScanner = new DeepFolderScanner(contentFolder, predicate: x => !VaultService.IsNameReserved(x.Name));
            var structureValidator = _unlockedVaultViewModel.StorageRoot.Options.HealthStatistics.StructureValidator;

            _healthModel = new HealthModel(folderScanner, new(this, this), structureValidator);
            _healthModel.IssueFound += HealthModel_IssueFound;
        }

        /// <inheritdoc/>
        public void Report(double value)
        {
            if (!IsProgressing)
                return;

            _context.PostOrExecute(_ => CurrentProgress = Math.Round(value));
        }

        /// <inheritdoc/>
        public void Report(TotalProgress value)
        {
            if (!IsProgressing)
                return;

            _context.PostOrExecute(_ =>
            {
                if (value.Total == 0)
                {
                    Title = value.Achieved == 0
                        ? "Collecting items..."
                        : $"Collecting items ({value.Achieved})";

                    StatusTitle = "Collecting items...";
                    Subtitle = value.Achieved == 0
                        ? "Items are being collected"
                        : $"Collected {value.Achieved} items";
                }
                else
                {
                    Title = value.Achieved == value.Total
                        ? "Scan completed"
                        : $"Scanning items ({value.Achieved} of {value.Total})";

                    StatusTitle = value.Achieved == value.Total
                        ? "Scan completed"
                        : "Scanning items";
                    Subtitle = $"Scanned {value.Achieved} out of {value.Total}";
                }
            });
        }

        partial void OnIsProgressingChanged(bool value)
        {
            _ = value;
            UpdateSeverity(FoundIssues);
        }

        private void UpdateSeverity(IEnumerable enumerable)
        {
#pragma warning disable MVVMTK0034
            var severity = Severity;
            if (severity != Enums.Severity.Success && FoundIssues.IsEmpty())
            {
                _Severity = Enums.Severity.Success;
                _context.PostOrExecute(_ => OnPropertyChanged(nameof(Severity)));
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
                _context.PostOrExecute(_ => OnPropertyChanged(nameof(Severity)));
            }
#pragma warning restore MVVMTK0034
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

        private async void HealthModel_IssueFound(object? sender, HealthIssueEventArgs e)
        {
            if (e.Result.Successful || e.Storable is null)
                return;

            var issueViewModel = await VaultHealthService.GetIssueViewModelAsync(e.Result, e.Storable).ConfigureAwait(false);
            _context.PostOrExecute(_ => FoundIssues.Add(issueViewModel ?? new(e.Storable, e.Result)));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _cts?.TryCancel();
            _cts?.Dispose();
            FoundIssues.CollectionChanged -= FoundIssues_CollectionChanged;
            if (_healthModel is not null)
            {
                _healthModel.IssueFound -= HealthModel_IssueFound;
                _healthModel.Dispose();
            }
        }
    }
}
