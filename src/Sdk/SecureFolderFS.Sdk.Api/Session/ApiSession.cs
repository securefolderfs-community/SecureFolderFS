using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Api.Enums;
using SecureFolderFS.Sdk.Api.Helpers;
using SecureFolderFS.Sdk.Api.Models;
using SecureFolderFS.Sdk.Api.Protocol;
using SecureFolderFS.Sdk.Api.Serialization;
using SecureFolderFS.Sdk.Api.Services;
using SecureFolderFS.Sdk.Api.Transport;
using static SecureFolderFS.Sdk.Api.Constants;

namespace SecureFolderFS.Sdk.Api.Session
{
    /// <summary>
    /// Serves one connected client for the lifetime of its connection.
    /// </summary>
    internal sealed class ApiSession : IAsyncDisposable
    {
        // A client that stops reading must not grow server memory without bound. On overflow a
        // notification is dropped, and the resulting revision gap tells the client to resynchronize
        private const int OUTBOUND_QUEUE_CAPACITY = 256;

        private static readonly string[] DefaultRequestedScopes =
        [
            Scopes.VAULTS_READ,
            Scopes.VAULTS_TRIGGER,
            Scopes.APP_CONTROL
        ];

        private readonly IApiConnection _connection;
        private readonly ApiSessionContext _context;
        private readonly NdjsonChannel _channel;
        private readonly Channel<ApiMessage> _outbound;
        private readonly PeerEvidence _evidence;

        private PairedClient? _client;
        private IReadOnlyList<string> _scopes = [];
        private string _clientName = "Unknown application";
        private IDisposable? _subscription;
        private bool _helloReceived;
        private bool _disposed;

        /// <summary>Gets whether this session has presented a valid token.</summary>
        public bool IsAuthenticated => _client is not null;

        public ApiSession(IApiConnection connection, ApiSessionContext context)
        {
            _connection = connection;
            _context = context;
            _channel = new NdjsonChannel(connection.Stream);
            _evidence = SafeDescribePeer(connection.Peer, context.EvidenceProvider);
            _outbound = Channel.CreateBounded<ApiMessage>(new BoundedChannelOptions(OUTBOUND_QUEUE_CAPACITY)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true
            });
        }

        /// <summary>
        /// Runs the read loop until the client disconnects or the host shuts down.
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using var sessionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var pumpTask = PumpOutboundAsync(sessionCts.Token);

            // A connection that never authenticates is dropped, so idle unpaired sockets cannot occupy
            // the small unauthenticated budget indefinitely
            using var handshakeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            handshakeCts.CancelAfter(TimeSpan.FromSeconds(Limits.HANDSHAKE_TIMEOUT_SECONDS));

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var readToken = IsAuthenticated ? cancellationToken : handshakeCts.Token;

                    ApiMessage? request;
                    try
                    {
                        request = await _channel.ReadAsync(readToken);
                    }
                    catch (ApiProtocolException ex)
                    {
                        await TrySendAsync(ApiMessage.Failure(null, ex.Code, ex.Message), cancellationToken);
                        if (ex.IsFatal)
                            break;

                        continue;
                    }

                    if (request is null)
                        break;

                    var response = await DispatchAsync(request, cancellationToken);
                    if (response is not null)
                        await TrySendAsync(response, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Shutdown, or the handshake deadline elapsed.
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException)
            {
                // The peer vanished mid-conversation.
            }
            finally
            {
                await sessionCts.CancelAsync();
                await Task.WhenAny(pumpTask, Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None));
            }
        }

        private async Task<ApiMessage?> DispatchAsync(ApiMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Method))
                return ApiMessage.Failure(request.Id, ErrorCodes.INVALID_REQUEST, "Missing 'method'.");

            // Requests without an identifier expect no reply
            if (request.Id is null)
                return null;

            var id = request.Id.Value;
            if (request.Method == Methods.HELLO)
                return await HandleHelloAsync(id, request, cancellationToken);

            if (!_helloReceived)
                return ApiMessage.Failure(id, ErrorCodes.UNAUTHORIZED, "Send 'hello' first.");

            if (request.Method == Methods.PAIR)
                return await HandlePairAsync(id, request, cancellationToken);

            if (!IsAuthenticated)
                return ApiMessage.Failure(id, ErrorCodes.PAIRING_REQUIRED, "This client is not paired.");

            var requiredScope = GetRequiredScope(request.Method);
            if (requiredScope is null)
                return ApiMessage.Failure(id, ErrorCodes.UNKNOWN_METHOD, $"Unknown method '{request.Method}'.");

            if (!_scopes.Contains(requiredScope))
                return ApiMessage.Failure(id, ErrorCodes.FORBIDDEN_SCOPE, $"Missing scope '{requiredScope}'.");

            var (isAllowed, retryAfterMs) = _context.RateLimiter.Check(GetRateLimitKey(), GetRequestClass(request.Method));
            if (!isAllowed)
                return ApiMessage.Failure(id, ErrorCodes.RATE_LIMITED, "Request budget exceeded.", retryAfterMs);

            if (!_context.Bridge.IsAvailable)
                return ApiMessage.Failure(id, ErrorCodes.INVALID_STATE, "The application is still starting up.");

            return request.Method switch
            {
                Methods.VAULTS_LIST => ApiMessage.Response(id, _context.Hub.GetSnapshot()),
                Methods.VAULTS_SUBSCRIBE => HandleSubscribe(id),
                Methods.VAULTS_UNSUBSCRIBE => HandleUnsubscribe(id),
                Methods.VAULTS_REQUEST_UNLOCK => await HandleVaultActionAsync(id, request, VaultAction.RequestUnlock, cancellationToken),
                Methods.VAULTS_LOCK => await HandleVaultActionAsync(id, request, VaultAction.Lock, cancellationToken),
                Methods.VAULTS_REVEAL => await HandleVaultActionAsync(id, request, VaultAction.Reveal, cancellationToken),
                Methods.APP_SHOW => await HandleShowAsync(id, cancellationToken),
                _ => ApiMessage.Failure(id, ErrorCodes.UNKNOWN_METHOD, $"Unknown method '{request.Method}'.")
            };
        }

        private async Task<ApiMessage> HandleHelloAsync(long id, ApiMessage request, CancellationToken cancellationToken)
        {
            HelloRequest? hello;
            try
            {
                hello = request.GetParams<HelloRequest>();
            }
            catch (Exception)
            {
                return ApiMessage.Failure(id, ErrorCodes.INVALID_PARAMS, "Malformed 'hello' parameters.");
            }

            if (hello is null)
                return ApiMessage.Failure(id, ErrorCodes.INVALID_PARAMS, "Missing 'hello' parameters.");

            // Ranges must overlap in both directions, otherwise neither side can be understood.
            if (hello.ProtocolMax < PROTOCOL_VERSION_MIN || hello.ProtocolMin > PROTOCOL_VERSION)
            {
                return ApiMessage.Failure(
                    id,
                    ErrorCodes.UNSUPPORTED_VERSION,
                    $"This build speaks protocol {PROTOCOL_VERSION_MIN}-{PROTOCOL_VERSION}.");
            }

            _helloReceived = true;
            _clientName = SanitizeClientName(hello.ClientName);

            var client = _context.PairingStore.Authenticate(hello.Token);
            if (client is not null)
            {
                _client = client;
                _scopes = client.Scopes;

                try
                {
                    await _context.PairingStore.TouchLastUsedAsync(client.Id, cancellationToken);
                }
                catch (Exception)
                {
                    // A failed write must not break authentication.
                }

                _clientName = client.DisplayName;
            }

            var state = IsAuthenticated
                ? SessionStates.PAIRED
                : SessionStates.PAIRING_REQUIRED;

            return ApiMessage.Response(id, new HelloResponse(
                PROTOCOL_VERSION,
                _context.AppVersion,
                state,
                Scopes.All,
                _scopes));
        }

        private async Task<ApiMessage> HandlePairAsync(long id, ApiMessage request, CancellationToken cancellationToken)
        {
            if (IsAuthenticated)
                return ApiMessage.Response(id, new PairResponse(string.Empty, _scopes));

            var fingerprint = ComputeFingerprint(_evidence.ExecutablePath, _clientName);
            if (_context.PairingStore.IsInDenialCooldown(fingerprint))
            {
                return ApiMessage.Failure(
                    id,
                    ErrorCodes.PAIRING_UNAVAILABLE,
                    "This application was recently refused and must wait before asking again.",
                    (int)_context.PairingStore.DenialCooldown.TotalMilliseconds);
            }

            var (isAllowed, retryAfterMs) = _context.RateLimiter.Check(fingerprint, ApiRequestType.Pairing);
            if (!isAllowed)
                return ApiMessage.Failure(id, ErrorCodes.RATE_LIMITED, "Too many pairing attempts.", retryAfterMs);

            var requestedScopes = NormalizeScopes(request.GetParams<PairRequest>()?.Scopes);

            // Only one consent prompt may be open at a time, so a burst of connections cannot stack dialogs.
            using var scope = _context.Coalescer.TryBeginScope("pairing");
            if (scope is null)
            {
                return ApiMessage.Failure(
                    id,
                    ErrorCodes.PAIRING_UNAVAILABLE,
                    "Another pairing request is already awaiting a decision.");
            }

            ApiConsentResult consent;
            try
            {
                consent = await _context.ConsentService.RequestConsentAsync(
                    new ApiConsentRequest(_clientName, _evidence, requestedScopes),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return ApiMessage.Failure(id, ErrorCodes.INTERNAL_ERROR, "The consent prompt could not be shown.");
            }

            if (!consent.IsGranted || consent.GrantedScopes.Count == 0)
            {
                await _context.PairingStore.RecordDenialAsync(fingerprint, cancellationToken);
                return ApiMessage.Failure(id, ErrorCodes.PAIRING_DENIED, "The user declined the request.");
            }

            var token = await _context.PairingStore.CreateClientAsync(_clientName, _evidence, consent.GrantedScopes, cancellationToken);

            _client = _context.PairingStore.Authenticate(token);
            _scopes = consent.GrantedScopes;

            return ApiMessage.Response(id, new PairResponse(token, consent.GrantedScopes));
        }

        private ApiMessage HandleSubscribe(long id)
        {
            _subscription?.Dispose();

            var (snapshot, subscription) = _context.Hub.Subscribe(TryEnqueueNotification);
            _subscription = subscription;

            return ApiMessage.Response(id, snapshot);
        }

        private ApiMessage HandleUnsubscribe(long id)
        {
            _subscription?.Dispose();
            _subscription = null;

            return ApiMessage.Response(id, new ActionResponse(ActionStatus.OK));
        }

        private async Task<ApiMessage> HandleVaultActionAsync(
            long id,
            ApiMessage request,
            VaultAction action,
            CancellationToken cancellationToken)
        {
            VaultRequest? parameters;
            try
            {
                parameters = request.GetParams<VaultRequest>();
            }
            catch (Exception)
            {
                return ApiMessage.Failure(id, ErrorCodes.INVALID_PARAMS, "Malformed parameters.");
            }

            if (string.IsNullOrEmpty(parameters?.VaultId))
                return ApiMessage.Failure(id, ErrorCodes.INVALID_PARAMS, "Missing 'vaultId'.");

            var vaultId = parameters.VaultId;
            using var scope = _context.Coalescer.TryBeginScope($"{action}:{vaultId}");
            if (scope is null)
                return ApiMessage.Response(id, new ActionResponse(ActionStatus.ALREADY_PENDING));

            var outcome = action switch
            {
                VaultAction.RequestUnlock => await _context.Bridge.RequestUnlockAsync(vaultId, cancellationToken),
                VaultAction.Lock => await _context.Bridge.LockAsync(vaultId, cancellationToken),
                VaultAction.Reveal => await _context.Bridge.RevealAsync(vaultId, cancellationToken),
                _ => ApiActionOutcome.Unavailable
            };

            return TranslateOutcome(id, outcome);
        }

        private async Task<ApiMessage> HandleShowAsync(long id, CancellationToken cancellationToken)
        {
            var outcome = await _context.Bridge.ShowMainWindowAsync(cancellationToken);
            return TranslateOutcome(id, outcome);
        }

        private static ApiMessage TranslateOutcome(long id, ApiActionOutcome outcome) => outcome switch
        {
            ApiActionOutcome.Ok => ApiMessage.Response(id, new ActionResponse(ActionStatus.OK)),
            ApiActionOutcome.AlreadyPending => ApiMessage.Response(id, new ActionResponse(ActionStatus.ALREADY_PENDING)),
            ApiActionOutcome.NoChange => ApiMessage.Response(id, new ActionResponse(ActionStatus.NO_CHANGE)),
            ApiActionOutcome.NotFound => ApiMessage.Failure(id, ErrorCodes.NOT_FOUND, "No such vault."),
            ApiActionOutcome.InvalidState => ApiMessage.Failure(id, ErrorCodes.INVALID_STATE, "The vault is not in a state that allows this."),
            _ => ApiMessage.Failure(id, ErrorCodes.INTERNAL_ERROR, "The request could not be completed.")
        };

        private bool TryEnqueueNotification(ApiMessage message)
            => _outbound.Writer.TryWrite(message);

        private async Task PumpOutboundAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var message in _outbound.Reader.ReadAllAsync(cancellationToken))
                    await _channel.WriteAsync(message, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Session ending.
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException)
            {
                // The peer went away; the read loop will notice and tear the session down.
            }
        }

        private async Task TrySendAsync(ApiMessage message, CancellationToken cancellationToken)
        {
            try
            {
                await _channel.WriteAsync(message, cancellationToken);
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException or OperationCanceledException)
            {
                // Losing a response to a vanished peer is not actionable.
            }
        }

        private string GetRateLimitKey()
        {
            // Keyed by pairing when known, else by the executable, so reconnecting does not reset budgets.
            return _client?.Id ?? ComputeFingerprint(_evidence.ExecutablePath, _clientName);
        }

        // Recognizes a caller across pairing attempts. Denial cooldowns and pre-pairing rate budgets are
        // keyed by this, so simply reconnecting cannot hand a misbehaving caller a fresh start.
        private static string ComputeFingerprint(string? executablePath, string clientName)
        {
            var material = string.IsNullOrEmpty(executablePath) ? $"name:{clientName}" : $"path:{executablePath}";
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(material)));
        }

        private static string? GetRequiredScope(string method) => method switch
        {
            Methods.VAULTS_LIST or
            Methods.VAULTS_SUBSCRIBE or
            Methods.VAULTS_UNSUBSCRIBE => Scopes.VAULTS_READ,

            Methods.VAULTS_REQUEST_UNLOCK or
            Methods.VAULTS_LOCK or
            Methods.VAULTS_REVEAL => Scopes.VAULTS_TRIGGER,

            Methods.APP_SHOW => Scopes.APP_CONTROL,

            _ => null
        };

        private static ApiRequestType GetRequestClass(string method) => method switch
        {
            Methods.VAULTS_LIST or
            Methods.VAULTS_SUBSCRIBE or
            Methods.VAULTS_UNSUBSCRIBE => ApiRequestType.Read,

            _ => ApiRequestType.Trigger
        };

        private static IReadOnlyList<string> NormalizeScopes(IReadOnlyList<string>? requested)
        {
            if (requested is null || requested.Count == 0)
                return DefaultRequestedScopes;

            // Silently discard unrecognized scopes so a client cannot inflate the prompt.
            var filtered = requested.Where(DefaultRequestedScopes.Contains).Distinct().ToArray();
            return filtered.Length == 0 ? DefaultRequestedScopes : filtered;
        }

        private static string SanitizeClientName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Unknown application";

            var trimmed = name.Trim();
            if (trimmed.Length > Limits.MAX_CLIENT_NAME_LENGTH)
                trimmed = trimmed[..Limits.MAX_CLIENT_NAME_LENGTH];

            // Control characters could forge line breaks or spoof surrounding text in the consent prompt.
            return new string(trimmed.Where(x => !char.IsControl(x)).ToArray());
        }

        private static PeerEvidence SafeDescribePeer(ApiPeerHandle peer, IPeerEvidenceProvider provider)
        {
            try
            {
                return provider.Describe(peer);
            }
            catch (Exception)
            {
                // Evidence is advisory, so failing to collect it must never refuse a connection.
                return PeerEvidence.Unknown;
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            _subscription?.Dispose();
            _subscription = null;

            _outbound.Writer.TryComplete();
            _channel.Dispose();

            await _connection.DisposeAsync();
        }

        private enum VaultAction
        {
            RequestUnlock,
            Lock,
            Reveal
        }
    }

    /// <summary>
    /// The services every session shares.
    /// </summary>
    internal sealed record ApiSessionContext(
        IVaultApiBridge Bridge,
        VaultNotificationHub Hub,
        IPairingStore PairingStore,
        ApiRateLimiterHelper RateLimiter,
        RequestCoalescerHelper Coalescer,
        IApiConsentService ConsentService,
        IPeerEvidenceProvider EvidenceProvider,
        string AppVersion);
}
