using SecureFolderFS.Core.Validators;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utilities;
using System.IO;

namespace SecureFolderFS.Core
{
    /// <summary>
    /// Provides helpers used for managing vaults and file systems.
    /// </summary>
    public static class VaultHelpers
    {
        public static IAsyncValidator<IFolder> NewVaultValidator(IAsyncSerializer<Stream> serializer)
        {
            return new VaultValidator(serializer);
        }
    }
}
