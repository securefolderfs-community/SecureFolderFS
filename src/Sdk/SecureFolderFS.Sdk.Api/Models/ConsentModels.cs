using System.Collections.Generic;

namespace SecureFolderFS.Sdk.Api.Models
{
    /// <summary>
    /// Describes what the operating system could actually prove about a connecting process.
    /// </summary>
    /// <param name="IsVerified">Whether the platform cryptographically verified the publisher. Always false on macOS and Linux.</param>
    /// <param name="ExecutablePath">The path of the connecting executable, when obtainable.</param>
    /// <param name="Signer">The verified publisher, present only when <paramref name="IsVerified"/> is true.</param>
    public sealed record PeerEvidence(
        bool IsVerified,
        string? ExecutablePath = null,
        string? Signer = null)
    {
        /// <summary>
        /// Gets an instance representing a peer about which nothing could be determined.
        /// </summary>
        public static PeerEvidence Unknown { get; } = new(IsVerified: false);
    }

    /// <summary>
    /// A request to let an application connect.
    /// </summary>
    /// <param name="ClientName">The client-supplied display name. Untrusted, so always shown next to <paramref name="Evidence"/>.</param>
    /// <param name="Evidence">What the OS could independently determine about the caller.</param>
    /// <param name="RequestedScopes">The permissions being asked for.</param>
    public sealed record ApiConsentRequest(
        string ClientName,
        PeerEvidence Evidence,
        IReadOnlyList<string> RequestedScopes);

    /// <summary>
    /// The user's answer to a consent prompt.
    /// </summary>
    /// <param name="IsGranted">Whether the user allowed the connection.</param>
    /// <param name="GrantedScopes">The permissions the user actually granted.</param>
    public sealed record ApiConsentResult(bool IsGranted, IReadOnlyList<string> GrantedScopes)
    {
        /// <summary>
        /// Gets a result representing a refusal.
        /// </summary>
        public static ApiConsentResult Denied { get; } = new(false, []);
    }
}
