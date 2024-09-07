using OwlCore.Storage;
using SecureFolderFS.Core.Migration.AppModels.V1_V2;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System.IO;

namespace SecureFolderFS.Core.Migration
{
    public static class Migrators
    {
        public static IVaultMigratorModel GetMigratorV1_V2(IFolder vaultFolder, IAsyncSerializer<Stream> streamSerializer)
        {
            return new MigratorV1_V2(vaultFolder, streamSerializer);
        }
    }
}
