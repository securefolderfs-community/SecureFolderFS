using SecureFolderFS.Core.Dokany;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.Routines.CreationRoutines;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;

namespace SecureFolderFS.Core
{
    /// <summary>
    /// Provides helpers used for managing vaults and file systems.
    /// </summary>
    public static class VaultHelpers
    {
        public static IUnlockRoutine NewUnlockRoutine()
        {
            return new UnlockRoutine();
        }

        public static ICreationRoutine NewCreationRoutine()
        {
            return new CreationRoutine();
        }

        public static IAsyncValidator<Stream> NewVersionValidator(IAsyncSerializer<Stream> serializer)
        {
            return new VersionValidator(serializer);
        }

        public static IAsyncValidator<IFolder> NewVaultValidator(IAsyncSerializer<Stream> serializer)
        {
            return new VaultValidator(serializer);
        }

        public static FileSystemAvailabilityType DetermineAvailability(FileSystemAdapterType adapterType)
        {
            return adapterType switch
            {
                FileSystemAdapterType.DokanAdapter => CheckAvailability<DokanyMountable>(),
                FileSystemAdapterType.WebDavAdapter => CheckAvailability<WebDavMountable>(),
                _ => throw new ArgumentOutOfRangeException(nameof(adapterType))
            };

            FileSystemAvailabilityType CheckAvailability<T>()
                where T : IAvailabilityChecker
            {
                return T.IsSupported();
            }
        }
    }
}
