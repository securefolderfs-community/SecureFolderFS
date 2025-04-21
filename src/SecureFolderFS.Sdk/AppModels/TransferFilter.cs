using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Storage.Pickers;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents a transfer type options.
    /// </summary>
    /// <param name="TransferType">The type of transfer to use.</param>
    public sealed record TransferOptions(TransferType TransferType) : PickerOptions;
}
