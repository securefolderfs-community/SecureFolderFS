using System.IO;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Storage.SystemStorageEx
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    public sealed class SystemFileSizeOfProperty(IStorable owner, FileInfo fileInfo)
        : SimpleStorageProperty<long?>(
            id: $"{owner.Id}/{nameof(ISizeOf.SizeOf)}",
            name: nameof(ISizeOf.SizeOf),
            getter: () => fileInfo.Length
        ), ISizeOfProperty;
}
