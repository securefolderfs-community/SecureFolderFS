using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Api.Helpers;
using SecureFolderFS.Sdk.Api.Models;
using SecureFolderFS.Sdk.Api.Session;
using SecureFolderFS.Sdk.Api.Transport;
using SecureFolderFS.Shared.Helpers;
using ApiConstants = SecureFolderFS.Sdk.Api.Constants;

namespace SecureFolderFS.Sdk.Api.Services
{
    /// <inheritdoc cref="IApiHost"/>
    public sealed class ApiHost : IApiHost
    {
        private readonly VaultNotificationHub _hub;
        private readonly ApiSessionContext _context;
        private readonly List<ApiSession> _sessions = [];
        private readonly Lock _lock = new();

        private CancellationTokenSource? _lifetimeCts;
        private IApiTransport? _transport;
        private Task? _acceptLoop;
        private bool _disposed;

        public ApiHost(
            IVaultApiBridge bridge,
            IPairingStore pairingStore,
            IApiConsentService consentService,
            IPeerEvidenceProvider evidenceProvider,
            string appVersion)
        {
            _hub = new VaultNotificationHub(bridge);
            _context = new ApiSessionContext(
                bridge,
                _hub,
                pairingStore,
                new ApiRateLimiterHelper(),
                new RequestCoalescerHelper(),
                consentService,
                evidenceProvider,
                appVersion);
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_transport is not null)
                return;

            var transport = CreateTransport();
            await transport.StartAsync(cancellationToken);

            _transport = transport;
            _lifetimeCts = new CancellationTokenSource();

            // Written only after the listener is bound, so a client woken by the file change finds something accepting on the other end
            PublishEndpoint(enabled: true);

            _acceptLoop = Task.Run(() => AcceptLoopAsync(transport, _lifetimeCts.Token), CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            var transport = _transport;
            if (transport is null)
                return;

            _transport = null;
            if (_lifetimeCts is not null)
                await _lifetimeCts.CancelAsync();

            await transport.DisposeAsync();
            if (_acceptLoop is not null)
                await Task.WhenAny(_acceptLoop, Task.Delay(TimeSpan.FromSeconds(2), cancellationToken));

            ApiSession[] sessions;
            lock (_lock)
            {
                sessions = _sessions.ToArray();
                _sessions.Clear();
            }

            foreach (var session in sessions)
                await SafetyHelpers.NoFailureAsync(async () => await session.DisposeAsync());

            _lifetimeCts?.Dispose();
            _lifetimeCts = null;
            _acceptLoop = null;
        }

        /// <inheritdoc/>
        public void PublishDisabled() => PublishEndpoint(enabled: false);

        private void PublishEndpoint(bool enabled)
        {
            try
            {
                var (kind, address) = ApiDiscoveryHelpers.ResolveEndpoint();
                ApiDiscoveryHelpers.Write(new ApiEndpointInfo(
                    ApiConstants.PROTOCOL_VERSION,
                    ApiDiscoveryHelpers.GetTransportName(kind),
                    address,
                    enabled,
                    ApiConstants.PROTOCOL_VERSION_MIN,
                    ApiConstants.PROTOCOL_VERSION,
                    Constants.Scopes.All,
                    _context.AppVersion,
                    SafetyHelpers.NoFailureResult(() => Environment.ProcessPath),
                    Environment.ProcessId));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The API still works for a client that already knows the address, so this is not fatal
            }
        }

        private async Task AcceptLoopAsync(IApiTransport transport, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                IApiConnection connection;
                try
                {
                    connection = await transport.AcceptAsync(cancellationToken);
                }
                catch (Exception ex) when (ex is OperationCanceledException or IOException or ObjectDisposedException)
                {
                    break;
                }
                catch (Exception)
                {
                    continue;
                }

                if (!TryAdmit())
                {
                    // Refused connections are closed immediately, so a client sees a clean disconnect
                    await SafetyHelpers.NoFailureAsync(async () => await connection.DisposeAsync());
                    continue;
                }

                var session = new ApiSession(connection, _context);
                lock (_lock)
                    _sessions.Add(session);

                _ = Task.Run(() => RunSessionAsync(session, cancellationToken), CancellationToken.None);
            }
        }

        /// <summary>
        /// Decides whether a new connection fits within the configured budgets.
        /// </summary>
        private bool TryAdmit()
        {
            lock (_lock)
            {
                if (_sessions.Count >= Constants.Limits.MAX_CONNECTIONS)
                    return false;

                // Unauthenticated sockets get their own small allowance so an unpaired caller cannot lock out paired clients.
                return _sessions.Count(x => !x.IsAuthenticated) < Constants.Limits.MAX_UNAUTHENTICATED_CONNECTIONS;
            }
        }

        private async Task RunSessionAsync(ApiSession session, CancellationToken cancellationToken)
        {
            try
            {
                await session.RunAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _ = ex;
            }
            finally
            {
                lock (_lock)
                    _sessions.Remove(session);

                await SafetyHelpers.NoFailureAsync(async () => await session.DisposeAsync());
            }
        }

        private static IApiTransport CreateTransport()
        {
            var (_, address) = ApiDiscoveryHelpers.ResolveEndpoint();
            if (!OperatingSystem.IsWindows())
                return new UnixSocketApiTransport(address);

            // The endpoint file advertises the full \\.\pipe\<name> form; the server API takes the bare name.
            var pipeName = address.StartsWith(@"\\.\pipe\", StringComparison.OrdinalIgnoreCase)
                ? address[@"\\.\pipe\".Length..]
                : address;

            return new NamedPipeApiTransport(pipeName);
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;
            await StopAsync();
            _hub.Dispose();
        }
    }
}
