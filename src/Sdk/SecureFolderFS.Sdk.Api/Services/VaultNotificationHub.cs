using System;
using System.Collections.Generic;
using System.Threading;
using SecureFolderFS.Sdk.Api.Enums;
using SecureFolderFS.Sdk.Api.EventArguments;
using SecureFolderFS.Sdk.Api.Models;
using SecureFolderFS.Sdk.Api.Protocol;

namespace SecureFolderFS.Sdk.Api.Services
{
    /// <summary>
    /// Assigns revisions to vault changes and distributes them out to subscribed sessions.
    /// </summary>
    /// <remarks>
    /// Every change increments a monotonic revision carried on snapshots and notifications, so a client
    /// that sees the revision jump by more than one knows it missed an event and can resynchronize.
    /// </remarks>
    public sealed class VaultNotificationHub : IDisposable
    {
        private readonly IVaultApiBridge _bridge;
        private readonly Lock _lock = new();
        private readonly Dictionary<Guid, Func<ApiMessage, bool>> _subscribers;
        private long _revision;
        private bool _disposed;

        public VaultNotificationHub(IVaultApiBridge bridge)
        {
            _bridge = bridge;
            _bridge.VaultChanged += OnVaultChanged;
            _subscribers = new();
        }

        /// <summary>
        /// Gets the current vault list without subscribing to further changes.
        /// </summary>
        public VaultListResponse GetSnapshot()
        {
            lock (_lock)
                return new VaultListResponse(_revision, _bridge.GetVaults());
        }

        /// <summary>
        /// Subscribes to changes and returns the snapshot those changes apply on top of. Registration and
        /// snapshot capture happen under one lock, so no change can fall between them.
        /// </summary>
        /// <param name="send">
        /// Receives notifications. Must not block. It is invoked while the hub lock is held, so it should
        /// enqueue rather than perform I/O. Returning <see langword="false"/> means the subscriber's queue
        /// overflowed, which surfaces to the client as a revision gap.
        /// </param>
        public (VaultListResponse Snapshot, IDisposable Subscription) Subscribe(Func<ApiMessage, bool> send)
        {
            lock (_lock)
            {
                var id = Guid.NewGuid();
                _subscribers[id] = send;

                var snapshot = new VaultListResponse(_revision, _bridge.GetVaults());
                return (snapshot, new HubSubscription(this, id));
            }
        }

        private void OnVaultChanged(object? sender, ApiVaultChangedEventArgs e)
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                var notification = CreateNotification(e, ++_revision);
                if (notification is null)
                    return;

                // Dispatched under the lock so notifications reach every subscriber in revision order.
                // Safe only because subscribers enqueue rather than write to the socket
                foreach (var subscriber in _subscribers.Values)
                    subscriber(notification);
            }
        }

        private static ApiMessage? CreateNotification(ApiVaultChangedEventArgs e, long revision)
        {
            if (e.Kind == ApiVaultChangeKind.Removed)
                return ApiMessage.Notification(Constants.Events.VAULT_REMOVED, new VaultRemovedNotification(revision, e.VaultId));

            if (e.Vault is null)
                return null;

            var method = e.Kind switch
            {
                ApiVaultChangeKind.Added => Constants.Events.VAULT_ADDED,
                ApiVaultChangeKind.Renamed => Constants.Events.VAULT_RENAMED,
                ApiVaultChangeKind.Unlocked => Constants.Events.VAULT_UNLOCKED,
                ApiVaultChangeKind.Locked => Constants.Events.VAULT_LOCKED,
                _ => null
            };

            return method is null
                ? null
                : ApiMessage.Notification(method, new VaultChangedNotification(revision, e.Vault));
        }

        private void Unsubscribe(Guid id)
        {
            lock (_lock)
                _subscribers.Remove(id);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;
                _bridge.VaultChanged -= OnVaultChanged;
                _subscribers.Clear();
            }
        }

        private sealed class HubSubscription(VaultNotificationHub hub, Guid id) : IDisposable
        {
            /// <inheritdoc/>
            public void Dispose() => hub.Unsubscribe(id);
        }
    }
}
