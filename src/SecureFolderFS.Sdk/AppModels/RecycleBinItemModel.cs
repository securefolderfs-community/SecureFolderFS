using System;
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed record class RecycleBinItemModel(
        string? PlaintextName,
        IStorableChild CiphertextItem,
        DateTime? DeletionTimestamp);
}
