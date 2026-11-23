using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Api.Enums;
using SecureFolderFS.Sdk.Api.EventArguments;
using SecureFolderFS.Sdk.Api.Helpers;
using SecureFolderFS.Sdk.Api.Models;
using SecureFolderFS.Sdk.Api.Services;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Views.Root;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using ApiConstants = SecureFolderFS.Sdk.Api.Constants;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <summary>
    /// Exposes live vault state to the local integration API.
    /// </summary>
    public sealed class UnoVaultApiBridge : IVaultApiBridge, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>, IDisposable
    {
        private readonly IPairingStore _pairingStore;
        private MainViewModel? _mainViewModel;
        private SynchronizationContext? _synchronizationContext;
        private VaultInfo[] _snapshot = [];
        private bool _disposed;

        /// <inheritdoc/>
        public bool IsAvailable => _mainViewModel is not null;

        /// <inheritdoc/>
        public event EventHandler<ApiVaultChangedEventArgs>? VaultChanged;

        /// <summary>
        /// Gets or sets the callback that shows the unlock prompt for a vault.
        /// </summary>
        public Func<VaultViewModel, Task<bool>>? ShowUnlockPromptAsync { get; set; }

        /// <summary>
        /// Gets or sets the callback that brings the main window to the foreground.
        /// </summary>
        public Func<Task>? ShowMainWindow { get; set; }

        public UnoVaultApiBridge(IPairingStore pairingStore)
        {
            _pairingStore = pairingStore;
        }

        /// <summary>
        /// Connects the bridge to live application state, once the main view model exists.
        /// </summary>
        public void Attach(MainViewModel mainViewModel, SynchronizationContext? synchronizationContext)
        {
            _mainViewModel = mainViewModel;
            _synchronizationContext = synchronizationContext;

            mainViewModel.VaultListViewModel.Items.CollectionChanged += Items_CollectionChanged;
            foreach (var item in mainViewModel.VaultListViewModel.Items)
                item.VaultViewModel.PropertyChanged += VaultViewModel_PropertyChanged;

            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);

            Rebuild();
        }

        /// <inheritdoc/>
        public IReadOnlyList<VaultInfo> GetVaults() => Volatile.Read(ref _snapshot);

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message) => Rebuild();

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message) => Rebuild();

        /// <summary>
        /// Recomputes the snapshot and raises an event for every difference.
        /// </summary>
        private void Rebuild()
        {
            if (_mainViewModel is null)
                return;

            var previous = Volatile.Read(ref _snapshot);
            var current = BuildSnapshot(_mainViewModel);
            Volatile.Write(ref _snapshot, current);

            if (VaultChanged is null)
                return;

            var previousById = previous.ToDictionary(x => x.Id);
            var currentById = current.ToDictionary(x => x.Id);

            foreach (var vault in current)
            {
                if (!previousById.TryGetValue(vault.Id, out var before))
                {
                    NotifyVaultChanged(ApiVaultChangeKind.Added, vault.Id, vault);
                    continue;
                }

                if (before.State != vault.State)
                {
                    var kind = vault.State == ApiConstants.VaultStates.UNLOCKED
                        ? ApiVaultChangeKind.Unlocked
                        : ApiVaultChangeKind.Locked;

                    NotifyVaultChanged(kind, vault.Id, vault);
                }
                else if (!string.Equals(before.Name, vault.Name, StringComparison.Ordinal))
                {
                    NotifyVaultChanged(ApiVaultChangeKind.Renamed, vault.Id, vault);
                }
            }

            foreach (var vault in previous)
            {
                if (!currentById.ContainsKey(vault.Id))
                    NotifyVaultChanged(ApiVaultChangeKind.Removed, vault.Id, null);
            }
        }

        private void NotifyVaultChanged(ApiVaultChangeKind kind, string vaultId, VaultInfo? vault)
            => VaultChanged?.Invoke(this, new ApiVaultChangedEventArgs(kind, vaultId, vault));

        private VaultInfo[] BuildSnapshot(MainViewModel mainViewModel)
        {
            var vaultIdKey = _pairingStore.VaultIdKey;
            var results = new List<VaultInfo>();

            foreach (var item in mainViewModel.VaultListViewModel.Items)
            {
                var vaultViewModel = item.VaultViewModel;
                var persistableId = vaultViewModel.VaultModel.DataModel.PersistableId;
                if (string.IsNullOrEmpty(persistableId))
                    continue;

                results.Add(new VaultInfo(
                    PublicVaultIdHelpers.Compute(vaultIdKey, persistableId),
                    vaultViewModel.Title ?? string.Empty,
                    vaultViewModel.IsUnlocked ? ApiConstants.VaultStates.UNLOCKED : ApiConstants.VaultStates.LOCKED,
                    TryGetMountPath(vaultViewModel),
                    vaultViewModel.LastAccessDate));
            }

            return results.ToArray();
        }

        private static string? TryGetMountPath(VaultViewModel vaultViewModel)
        {
            if (!vaultViewModel.IsUnlocked)
                return null;

            try
            {
                return vaultViewModel.GetUnlockedViewModel().StorageRoot.VirtualizedRoot.Id;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Task<ApiActionOutcome> RequestUnlockAsync(string vaultId, CancellationToken cancellationToken = default)
        {
            return InvokeOnUiThreadAsync(async () =>
            {
                if (!TryResolve(vaultId, out var vaultViewModel))
                    return ApiActionOutcome.NotFound;

                if (vaultViewModel.IsUnlocked)
                    return ApiActionOutcome.NoChange;

                if (ShowUnlockPromptAsync is null)
                    return ApiActionOutcome.Unavailable;

                var shown = await ShowUnlockPromptAsync(vaultViewModel);
                return shown ? ApiActionOutcome.Ok : ApiActionOutcome.AlreadyPending;
            });
        }

        /// <inheritdoc/>
        public Task<ApiActionOutcome> LockAsync(string vaultId, CancellationToken cancellationToken = default)
        {
            return InvokeOnUiThreadAsync(() =>
            {
                if (!TryResolve(vaultId, out var vaultViewModel))
                    return Task.FromResult(ApiActionOutcome.NotFound);

                if (!vaultViewModel.IsUnlocked)
                    return Task.FromResult(ApiActionOutcome.NoChange);

                WeakReferenceMessenger.Default.Send(new VaultLockRequestedMessage(vaultViewModel.VaultModel));
                return Task.FromResult(ApiActionOutcome.Ok);
            });
        }

        /// <inheritdoc/>
        public Task<ApiActionOutcome> RevealAsync(string vaultId, CancellationToken cancellationToken = default)
        {
            return InvokeOnUiThreadAsync(async () =>
            {
                if (!TryResolve(vaultId, out var vaultViewModel))
                    return ApiActionOutcome.NotFound;

                if (!vaultViewModel.IsUnlocked)
                    return ApiActionOutcome.InvalidState;

                var fileExplorerService = DI.OptionalService<IFileExplorerService>();
                if (fileExplorerService is null)
                    return ApiActionOutcome.Unavailable;

                var unlocked = vaultViewModel.GetUnlockedViewModel();
                await fileExplorerService.TryOpenInFileExplorerAsync(unlocked.StorageRoot.VirtualizedRoot, cancellationToken);

                return ApiActionOutcome.Ok;
            });
        }

        /// <inheritdoc/>
        public Task<ApiActionOutcome> ShowMainWindowAsync(CancellationToken cancellationToken = default)
        {
            return InvokeOnUiThreadAsync(async () =>
            {
                if (ShowMainWindow is null)
                    return ApiActionOutcome.Unavailable;

                await ShowMainWindow();
                return ApiActionOutcome.Ok;
            });
        }

        private bool TryResolve(string vaultId, out VaultViewModel vaultViewModel)
        {
            vaultViewModel = null!;

            if (_mainViewModel is null)
                return false;

            var vaultIdKey = _pairingStore.VaultIdKey;
            foreach (var item in _mainViewModel.VaultListViewModel.Items)
            {
                var persistableId = item.VaultViewModel.VaultModel.DataModel.PersistableId;
                if (string.IsNullOrEmpty(persistableId))
                    continue;

                if (!string.Equals(PublicVaultIdHelpers.Compute(vaultIdKey, persistableId), vaultId, StringComparison.Ordinal))
                    continue;

                vaultViewModel = item.VaultViewModel;
                return true;
            }

            return false;
        }

        private async Task<ApiActionOutcome> InvokeOnUiThreadAsync(Func<Task<ApiActionOutcome>> action)
        {
            if (_mainViewModel is null)
                return ApiActionOutcome.Unavailable;

            var outcome = ApiActionOutcome.Unavailable;
            await _synchronizationContext.PostOrExecuteAsync(async () =>
            {
                try
                {
                    outcome = await action();
                }
                catch (Exception)
                {
                    outcome = ApiActionOutcome.Unavailable;
                }
            });

            return outcome;
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.OldItems?.OfType<VaultListItemViewModel>() ?? [])
                item.VaultViewModel.PropertyChanged -= VaultViewModel_PropertyChanged;

            foreach (var item in e.NewItems?.OfType<VaultListItemViewModel>() ?? [])
                item.VaultViewModel.PropertyChanged += VaultViewModel_PropertyChanged;

            Rebuild();
        }

        private void VaultViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(VaultViewModel.Title) or nameof(VaultViewModel.IsUnlocked))
                Rebuild();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            WeakReferenceMessenger.Default.UnregisterAll(this);

            if (_mainViewModel is not null)
            {
                _mainViewModel.VaultListViewModel.Items.CollectionChanged -= Items_CollectionChanged;
                foreach (var item in _mainViewModel.VaultListViewModel.Items)
                    item.VaultViewModel.PropertyChanged -= VaultViewModel_PropertyChanged;
            }

            _mainViewModel = null;
        }
    }
}
