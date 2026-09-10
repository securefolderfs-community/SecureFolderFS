using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Api.Services;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="ILocalIntegrationsService"/>
    public sealed class UnoLocalIntegrationsService : ILocalIntegrationsService
    {
        private readonly IApiHost _apiHost;
        private readonly IPairingStore _pairingStore;
        private readonly ISettingsService _settingsService;

        public UnoLocalIntegrationsService(IApiHost apiHost, IPairingStore pairingStore, ISettingsService settingsService)
        {
            _apiHost = apiHost;
            _pairingStore = pairingStore;
            _settingsService = settingsService;

            // A pairing is created off the UI thread by an API session, so notify the settings UI to refresh
            _pairingStore.ClientsChanged += PairingStore_ClientsChanged;
        }

        private static void PairingStore_ClientsChanged(object? sender, EventArgs e)
        {
            App.Instance?.MainWindowSynchronizationContext?.Post(_ => WeakReferenceMessenger.Default.Send(new IntegrationClientsChangedMessage()), null);
        }

        /// <inheritdoc/>
        public async Task SetEnabledAsync(bool isEnabled, CancellationToken cancellationToken = default)
        {
            _settingsService.UserSettings.EnableLocalIntegrations = isEnabled;
            await _settingsService.UserSettings.TrySaveAsync(cancellationToken);

            if (isEnabled)
            {
                await _apiHost.StartAsync(cancellationToken);
                return;
            }

            await _apiHost.StopAsync(cancellationToken);

            // Recorded in the endpoint file so a client can tell "switched off" apart from "not running",
            // and explain to its user why nothing is showing.
            _apiHost.PublishDisabled();
        }

        /// <inheritdoc/>
        public IReadOnlyList<IntegrationClientInfo> GetClients()
        {
            return _pairingStore.GetClients()
                .Select(x => new IntegrationClientInfo(
                    x.Id,
                    x.DisplayName,
                    x.WasIdentityVerified,
                    x.Signer,
                    x.ExecutablePath,
                    x.LastUsedAt))
                .ToArray();
        }

        /// <inheritdoc/>
        public Task RevokeClientAsync(string clientId, CancellationToken cancellationToken = default)
            => _pairingStore.RevokeClientAsync(clientId, cancellationToken);

        /// <inheritdoc/>
        public Task RevokeAllClientsAsync(CancellationToken cancellationToken = default)
            => _pairingStore.RevokeAllAsync(cancellationToken);
    }
}
