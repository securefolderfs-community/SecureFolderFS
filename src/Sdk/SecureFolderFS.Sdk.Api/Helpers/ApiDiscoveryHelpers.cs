using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using SecureFolderFS.Sdk.Api.Enums;
using SecureFolderFS.Sdk.Api.Models;
using SecureFolderFS.Sdk.Api.Serialization;

namespace SecureFolderFS.Sdk.Api.Helpers
{
    /// <summary>
    /// Resolves where the API endpoint lives on each platform, and reads or writes the endpoint file.
    /// </summary>
    public static class ApiDiscoveryHelpers
    {
        // sockaddr_un.sun_path is 104 bytes on macOS and 108 on Linux
        private const int MAX_SOCKET_PATH_BYTES = 104;
        private const string SOCKET_FILE_NAME = "api-v1.sock";

        /// <summary>
        /// Determines the transport and address this machine should use.
        /// </summary>
        public static (ApiTransportKind Kind, string Address) ResolveEndpoint()
        {
            return OperatingSystem.IsWindows()
                ? (ApiTransportKind.NamedPipe, GetPipeName())
                : (ApiTransportKind.UnixSocket, GetSocketPath());
        }

        // The SID is hashed rather than embedded so the pipe name, visible machine-wide, leaks no account id.
        [SupportedOSPlatform("windows")]
        private static string GetPipeName()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var identitySid = identity.User?.Value ?? identity.Name;
            var digest = SHA256.HashData(Encoding.UTF8.GetBytes(identitySid));
            var suffix = Convert.ToHexString(digest, 0, 8).ToLowerInvariant();

            return $"{Constants.APP_DIRECTORY_NAME}.Api.v{Constants.PROTOCOL_VERSION}.{suffix}";
        }

        private static string GetSocketPath()
        {
            foreach (var directory in EnumerateSocketDirectoryCandidates())
            {
                if (string.IsNullOrEmpty(directory))
                    continue;

                var candidate = Path.Combine(directory, SOCKET_FILE_NAME);
                if (Encoding.UTF8.GetByteCount(candidate) + 1 <= MAX_SOCKET_PATH_BYTES)
                    return candidate;
            }

            throw new IOException("Unable to find a location for the API socket that fits within the platform path limit.");
        }

        /// <summary>
        /// Yields socket directories in order of preference.
        /// </summary>
        private static string?[] EnumerateSocketDirectoryCandidates()
        {
            if (OperatingSystem.IsMacOS())
            {
                return
                [
                    // $TMPDIR is per-user (mode 0700), the right home for runtime state, and short enough
                    // to stay within the socket path limit whatever the account is called.
                    CombineIfPresent(Path.GetTempPath(), Constants.XDG_DIRECTORY_NAME),

                    // Application Support is conventionally world-readable, so use a subdirectory we own.
                    Path.Combine(GetPersistentDirectory(), "api")
                ];
            }

            return
            [
                // Runtime state belongs in XDG_RUNTIME_DIR, which is per-user and already mode 0700.
                CombineIfPresent(Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR"), Constants.XDG_DIRECTORY_NAME),

                // Sessions without a runtime directory (plain SSH, for instance) still need somewhere.
                CombineIfPresent(Path.GetTempPath(), $"{Constants.XDG_DIRECTORY_NAME}-{Environment.UserName}")
            ];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static string? CombineIfPresent(string? root, string child)
                => string.IsNullOrEmpty(root) ? null : Path.Combine(root, child);
        }

        /// <summary>
        /// Gets the per-user directory that holds state surviving application exit.
        /// </summary>
        private static string GetPersistentDirectory()
        {
            if (OperatingSystem.IsWindows())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Constants.APP_DIRECTORY_NAME);

            if (OperatingSystem.IsMacOS())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", Constants.APP_DIRECTORY_NAME);

            var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (string.IsNullOrEmpty(configHome))
                configHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

            return Path.Combine(configHome, Constants.XDG_DIRECTORY_NAME);
        }

        /// <summary>
        /// Writes the endpoint file atomically, so a polling client never observes a partial descriptor.
        /// </summary>
        public static void Write(ApiEndpointInfo endpointInfo)
        {
            var endpointPath = Path.Combine(GetPersistentDirectory(), Constants.ENDPOINT_FILE_NAME);
            var directory = Path.GetDirectoryName(endpointPath)!;

            // Default permissions on purpose because the directory may be shared and the file holds no secrets
            Directory.CreateDirectory(directory);

            var temporaryPath = $"{endpointPath}.tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(endpointInfo, ApiSerializer.Options));
            File.Move(temporaryPath, endpointPath, overwrite: true);
        }

        /// <summary>
        /// Gets the transport identifier written into the endpoint file.
        /// </summary>
        public static string GetTransportName(ApiTransportKind kind) => kind switch
        {
            ApiTransportKind.NamedPipe => Constants.TRANSPORT_NAMED_PIPE,
            ApiTransportKind.UnixSocket => Constants.TRANSPORT_UNIX_SOCKET,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }
}
