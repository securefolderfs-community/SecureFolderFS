using SecureFolderFS.Sdk.Api.Transport;
using SecureFolderFS.Sdk.Api.Models;

namespace SecureFolderFS.Sdk.Api.Services
{
    /// <summary>
    /// Gathers best-effort identity information about a connecting process.
    /// </summary>
    public interface IPeerEvidenceProvider
    {
        /// <summary>
        /// Describes the process on the other end of a connection, or <see cref="PeerEvidence.Unknown"/>.
        /// </summary>
        PeerEvidence Describe(ApiPeerHandle peer);
    }
}
