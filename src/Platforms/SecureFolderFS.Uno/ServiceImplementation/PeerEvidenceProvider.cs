using System;
#if WINDOWS
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32.SafeHandles;
using SecureFolderFS.Sdk.Api.Enums;
#elif __UNO_SKIA_MACOS__
using System.Text;
#else
using System.IO;
#endif
using SecureFolderFS.Sdk.Api.Transport;
using SecureFolderFS.Sdk.Api.Models;
using SecureFolderFS.Sdk.Api.Services;
using SecureFolderFS.Uno.PInvoke;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <summary>
    /// Gathers what each operating system can tell us about a connecting process. Windows can verify a
    /// publisher signature; macOS and Linux cannot, and say so. Used only to inform the consent prompt and
    /// never to authorize a request, as every identifier here is spoofable or race-prone.
    /// </summary>
    public sealed class PeerEvidenceProvider : IPeerEvidenceProvider
    {
#if __UNO_SKIA_MACOS__
        private const int PROC_PID_PATH_MAX = 4096;

        // macOS: SOL_LOCAL / LOCAL_PEERPID, returning a single pid
        private const int SOL_LOCAL_MACOS = 0;
        private const int LOCAL_PEERPID_MACOS = 2;
#elif !WINDOWS
        // Linux: SOL_SOCKET / SO_PEERCRED, returning struct ucred { pid, uid, gid }
        private const int SOL_SOCKET_LINUX = 1;
        private const int SO_PEERCRED_LINUX = 17;
#endif

        /// <inheritdoc/>
        public PeerEvidence Describe(ApiPeerHandle peer)
        {
            var processId = TryGetProcessId(peer);
            if (processId is null)
                return PeerEvidence.Unknown;

            var executablePath = TryGetExecutablePath(processId.Value);

#if WINDOWS
            // Only Windows can attest to who published the executable
            if (executablePath is not null)
            {
                var signer = TryGetAuthenticodeSigner(executablePath);
                if (signer is not null)
                    return new PeerEvidence(IsVerified: true, executablePath, signer);
            }
#endif

            // Elsewhere the consent prompt states plainly that the identity could not be verified
            return new PeerEvidence(IsVerified: false, executablePath);
        }

        private static int? TryGetProcessId(ApiPeerHandle peer)
        {
            try
            {
#if WINDOWS
                if (peer.Kind == ApiTransportKind.NamedPipe && peer.PipeHandle is { } pipeHandle)
                    return TryGetPipeClientProcessId(pipeHandle);
#else
                if (peer.Socket is { } socket)
                    return TryGetSocketPeerProcessId((int)socket.Handle);
#endif
            }
            catch (Exception)
            {
                // Evidence is advisory. Failing to collect it must never refuse a connection
            }

            return null;
        }

#if WINDOWS
        [SupportedOSPlatform("windows")]
        private static int? TryGetPipeClientProcessId(SafePipeHandle pipeHandle)
            => UnsafeNative.GetNamedPipeClientProcessId(pipeHandle, out var clientProcessId) ? (int)clientProcessId : null;
#else
        private static int? TryGetSocketPeerProcessId(int fileDescriptor)
        {
#if __UNO_SKIA_MACOS__
            var buffer = new byte[4];
            var length = buffer.Length;
            if (UnsafeNative.getsockopt(fileDescriptor, SOL_LOCAL_MACOS, LOCAL_PEERPID_MACOS, buffer, ref length) != 0)
                return null;

            return BitConverter.ToInt32(buffer, 0);
#else
            var credentials = new byte[12];
            var length = credentials.Length;
            if (UnsafeNative.getsockopt(fileDescriptor, SOL_SOCKET_LINUX, SO_PEERCRED_LINUX, credentials, ref length) != 0)
                return null;

            return BitConverter.ToInt32(credentials, 0);
#endif
        }
#endif

        private static string? TryGetExecutablePath(int processId)
        {
            try
            {
#if WINDOWS
                using var process = Process.GetProcessById(processId);
                return process.MainModule?.FileName;
#elif __UNO_SKIA_MACOS__
                var buffer = new byte[PROC_PID_PATH_MAX];
                var length = UnsafeNative.proc_pidpath(processId, buffer, (uint)buffer.Length);

                return length > 0 ? Encoding.UTF8.GetString(buffer, 0, length) : null;
#else
                return File.ResolveLinkTarget($"/proc/{processId}/exe", returnFinalTarget: true)?.FullName;
#endif
            }
            catch (Exception)
            {
                // The process may have exited, or be inaccessible to us
            }

            return null;
        }

#if WINDOWS
        [SupportedOSPlatform("windows")]
        private static string? TryGetAuthenticodeSigner(string executablePath)
        {
            try
            {
                // X509CertificateLoader has no path that extracts an Authenticode signer from a PE, so the
                // signer certificate is read from the signed file and then parsed through the loader.
#pragma warning disable SYSLIB0057 // CreateFromSignedFile is the only BCL Authenticode reader
                using var signerCertificate = X509Certificate.CreateFromSignedFile(executablePath);
#pragma warning restore SYSLIB0057
                using var certificate = X509CertificateLoader.LoadCertificate(signerCertificate.GetRawCertData());

                var commonName = certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
                return string.IsNullOrWhiteSpace(commonName) ? null : commonName;
            }
            catch (Exception)
            {
                // Unsigned, or the signature could not be read. Either way the caller is unverified.
                return null;
            }
        }
#endif
    }
}
