using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace SecureFolderFS.Cli;

/// <summary>
/// Talks to a running SecureFolderFS instance over the local integration API.
/// </summary>
internal sealed class AppApiClient : IAsyncDisposable
{
    private const string CLIENT_NAME = "SecureFolderFS CLI";
    private const int PROTOCOL_VERSION = 1;

    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

    private readonly Stream _stream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private long _nextId = 1;

    private AppApiClient(Stream stream)
    {
        _stream = stream;
        _reader = new StreamReader(stream, Encoding.UTF8);
        // Newline-delimited JSON
        _writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };
    }

    /// <summary>
    /// Connects to the running application, pairing if this is the first use.
    /// </summary>
    /// <exception cref="AppApiException">The API was unreachable, or pairing was refused.</exception>
    public static async Task<AppApiClient> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = ApiEndpoint.TryRead()
            ?? throw new AppApiException("SecureFolderFS does not appear to be installed for this user.");

        if (!endpoint.Enabled)
            throw new AppApiException("App integrations are turned off. Enable them in SecureFolderFS under Settings > Preferences.");

        if (PROTOCOL_VERSION < endpoint.ProtocolMin || PROTOCOL_VERSION > endpoint.ProtocolMax)
            throw new AppApiException("SecureFolderFS speaks a protocol version this build does not support.");

        var stream = await endpoint.OpenAsync(ConnectTimeout, cancellationToken)
            ?? throw new AppApiException("SecureFolderFS is not running.");

        var client = new AppApiClient(stream);
        try
        {
            await client.HandshakeAsync(cancellationToken);
            return client;
        }
        catch
        {
            await client.DisposeAsync();
            throw;
        }
    }

    private async Task HandshakeAsync(CancellationToken cancellationToken)
    {
        var token = TokenStore.TryRead();
        var hello = await CallAsync("hello", new
        {
            protocolMin = PROTOCOL_VERSION,
            protocolMax = PROTOCOL_VERSION,
            clientName = CLIENT_NAME,
            token
        }, cancellationToken);

        if (hello.GetProperty("state").GetString() == "paired")
            return;

        // A stored token that no longer authenticates means the user revoked us.
        if (token is not null)
            TokenStore.Clear();

        // 'pair' raises a consent dialog
        var pair = await CallAsync("pair", new { scopes = new[] { "vaults.read", "vaults.trigger" } }, cancellationToken);
        TokenStore.Write(pair.GetProperty("token").GetString()!);
    }

    /// <summary>
    /// Lists every vault visible to integrations.
    /// </summary>
    public async Task<IReadOnlyList<ApiVault>> ListVaultsAsync(CancellationToken cancellationToken = default)
    {
        var result = await CallAsync("vaults.list", null, cancellationToken);

        var vaults = new List<ApiVault>();
        foreach (var vault in result.GetProperty("vaults").EnumerateArray())
        {
            vaults.Add(new ApiVault(
                vault.GetProperty("id").GetString() ?? string.Empty,
                vault.GetProperty("name").GetString() ?? string.Empty,
                vault.GetProperty("state").GetString() ?? string.Empty,
                vault.TryGetProperty("mountPath", out var mountPath) ? mountPath.GetString() : null));
        }

        return vaults;
    }

    /// <summary>
    /// Asks the application to show its unlock prompt for a vault. Returns the reported status.
    /// </summary>
    public Task<string> RequestUnlockAsync(string vaultId, CancellationToken cancellationToken = default)
        => CallForStatusAsync("vaults.requestUnlock", vaultId, cancellationToken);

    /// <summary>
    /// Locks an unlocked vault. Returns the reported status.
    /// </summary>
    public Task<string> LockAsync(string vaultId, CancellationToken cancellationToken = default)
        => CallForStatusAsync("vaults.lock", vaultId, cancellationToken);

    private async Task<string> CallForStatusAsync(string method, string vaultId, CancellationToken cancellationToken)
    {
        var result = await CallAsync(method, new { vaultId }, cancellationToken);
        return result.TryGetProperty("status", out var status) ? status.GetString() ?? "ok" : "ok";
    }

    private async Task<JsonElement> CallAsync(string method, object? parameters, CancellationToken cancellationToken)
    {
        var id = _nextId++;
        await _writer.WriteLineAsync(JsonSerializer.Serialize(new { id, method, @params = parameters }));

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(RequestTimeout);

        while (true)
        {
            var line = await _reader.ReadLineAsync(timeout.Token)
                ?? throw new AppApiException($"SecureFolderFS closed the connection during '{method}'.");

            if (line.Length == 0)
                continue;

            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;

            // Notifications carry no id, and replies to other requests may interleave with this one.
            if (!root.TryGetProperty("id", out var replyId) || replyId.ValueKind == JsonValueKind.Null || replyId.GetInt64() != id)
                continue;

            if (root.TryGetProperty("error", out var error))
                throw new AppApiException(DescribeError(method, error));

            return root.TryGetProperty("result", out var result) ? result.Clone() : default;
        }
    }

    private static string DescribeError(string method, JsonElement error)
    {
        var code = error.TryGetProperty("code", out var c) ? c.GetString() : null;
        var message = error.TryGetProperty("message", out var m) ? m.GetString() : null;
        var retryAfterMs = error.TryGetProperty("retryAfterMs", out var r) && r.ValueKind == JsonValueKind.Number ? r.GetInt32() : 0;

        return code switch
        {
            "pairing_denied" => "SecureFolderFS declined the connection.",
            "pairing_unavailable" => "SecureFolderFS is not accepting a pairing request right now. Try again shortly.",
            "forbidden_scope" => "This tool was not granted permission for that action.",
            "rate_limited" => $"Rate limited; retry in {retryAfterMs / 1000}s.",
            "not_found" => "No such vault.",
            _ => $"'{method}' failed ({code}): {message}"
        };
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _reader.Dispose();
        await _writer.DisposeAsync();
        await _stream.DisposeAsync();
    }

    /// <summary>
    /// A vault as reported by the API.
    /// </summary>
    internal sealed record ApiVault(string Id, string Name, string State, string? MountPath);

    /// <summary>
    /// Locates and connects to the endpoint by reading the persistent endpoint file, exactly as the
    /// protocol specification describes.
    /// </summary>
    private sealed record ApiEndpoint(string Transport, string Address, bool Enabled, int ProtocolMin, int ProtocolMax)
    {
        public static ApiEndpoint? TryRead()
        {
            try
            {
                var path = Path.Combine(GetPersistentDirectory(), "api-endpoint.json");
                if (!File.Exists(path))
                    return null;

                using var document = JsonDocument.Parse(File.ReadAllText(path));
                var root = document.RootElement;

                return new ApiEndpoint(
                    root.GetProperty("transport").GetString() ?? string.Empty,
                    root.GetProperty("address").GetString() ?? string.Empty,
                    root.TryGetProperty("enabled", out var enabled) && enabled.GetBoolean(),
                    root.TryGetProperty("protocolMin", out var min) ? min.GetInt32() : 0,
                    root.TryGetProperty("protocolMax", out var max) ? max.GetInt32() : 0);
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        /// <summary>
        /// Opens the transport, or <see langword="null"/> when nothing is listening.
        /// </summary>
        public async Task<Stream?> OpenAsync(TimeSpan connectTimeout, CancellationToken cancellationToken)
        {
            try
            {
                return Transport switch
                {
                    "namedPipe" => await ConnectPipeAsync(Address, connectTimeout, cancellationToken),
                    "unixSocket" => await ConnectSocketAsync(Address, cancellationToken),
                    _ => null
                };
            }
            catch (Exception ex) when (ex is SocketException or IOException or TimeoutException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        private static async Task<Stream> ConnectPipeAsync(string address, TimeSpan connectTimeout, CancellationToken cancellationToken)
        {
            // On Windows the advertised address is the bare pipe name; tolerate a \\.\pipe\ prefix too.
            var pipeName = address.StartsWith(@"\\.\pipe\", StringComparison.OrdinalIgnoreCase)
                ? address[@"\\.\pipe\".Length..]
                : address;

            var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(connectTimeout);

                await pipe.ConnectAsync(timeout.Token);
                return pipe;
            }
            catch
            {
                await pipe.DisposeAsync();
                throw;
            }
        }

        private static async Task<Stream?> ConnectSocketAsync(string address, CancellationToken cancellationToken)
        {
            if (!File.Exists(address))
                return null;

            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                await socket.ConnectAsync(new UnixDomainSocketEndPoint(address), cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        private static string GetPersistentDirectory()
        {
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SecureFolderFS");
            }

            if (OperatingSystem.IsMacOS())
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library", "Application Support", "SecureFolderFS");
            }

            var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (string.IsNullOrEmpty(configHome))
                configHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

            return Path.Combine(configHome, "securefolderfs");
        }
    }

    /// <summary>
    /// Persists the pairing token for this tool.
    /// </summary>
    private static class TokenStore
    {
        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".securefolderfs",
            "cli-api-token");

        public static string? TryRead()
        {
            try
            {
                return File.Exists(FilePath) ? File.ReadAllText(FilePath).Trim() : null;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        public static void Write(string token)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, token);

                // The token authorizes this tool, so keep it readable only by its owner.
                if (!OperatingSystem.IsWindows())
                    File.SetUnixFileMode(FilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Losing the token only means pairing again next time.
            }
        }

        public static void Clear()
        {
            try
            {
                File.Delete(FilePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Best effort.
            }
        }
    }
}

/// <summary>
/// Represents a failure talking to the local integration API.
/// </summary>
internal sealed class AppApiException(string message) : Exception(message);
