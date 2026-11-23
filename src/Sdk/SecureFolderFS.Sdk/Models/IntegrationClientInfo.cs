using System;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// Describes an application the user has authorized to use the local integration API.
    /// </summary>
    /// <param name="Id">The identifier of the pairing, used to revoke it.</param>
    /// <param name="DisplayName">The name shown to the user.</param>
    /// <param name="IsIdentityVerified">Whether the platform verified the publisher when it was authorized.</param>
    /// <param name="Signer">The verified publisher, when one was established.</param>
    /// <param name="ExecutablePath">Where the application lived when it was authorized.</param>
    /// <param name="LastUsedAt">When it last connected, or <see langword="null"/> if it never has.</param>
    public sealed record IntegrationClientInfo(
        string Id,
        string DisplayName,
        bool IsIdentityVerified,
        string? Signer,
        string? ExecutablePath,
        DateTimeOffset? LastUsedAt);
}
