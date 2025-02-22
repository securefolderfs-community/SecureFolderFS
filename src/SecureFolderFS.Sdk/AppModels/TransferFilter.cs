using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents a transfer type filter.
    /// </summary>
    /// <param name="TransferType">The type of transfer to use.</param>
    public sealed record TransferFilter(TransferType TransferType) : FilterOptions;
}
