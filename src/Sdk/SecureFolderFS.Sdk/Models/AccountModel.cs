using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Models
{
    /// <summary>
    /// A provider-agnostic, UI-facing description of an account managed on this device
    /// </summary>
    /// <param name="Id">The stable, unique identifier of the account within its provider.</param>
    /// <param name="DisplayName">The human-friendly display name (typically the user's email).</param>
    /// <param name="Subtitle">An optional secondary line (e.g., the server URL).</param>
    /// <param name="Icon">An optional icon representing the account or its provider.</param>
    /// <param name="ProviderId">The id of the <see cref="IAccountProvider"/> that owns this account.</param>
    public sealed record AccountModel(string Id, string? DisplayName, string? Subtitle, IImage? Icon, string ProviderId);
}
