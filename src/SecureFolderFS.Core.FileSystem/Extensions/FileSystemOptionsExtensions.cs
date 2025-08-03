using System;
using System.Collections.Generic;
using SecureFolderFS.Core.FileSystem.Validators;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Extensions
{
    public static class FileSystemOptionsExtensions
    {
        public static void SetupValidators(this VirtualFileSystemOptions fileSystemOptions, FileSystemSpecifics specifics)
        {
            fileSystemOptions.HealthStatistics.FileValidator ??= new FileValidator(specifics);
            fileSystemOptions.HealthStatistics.FolderValidator ??= new FolderValidator(specifics);
            fileSystemOptions.HealthStatistics.StructureValidator ??= new StructureValidator(specifics, fileSystemOptions.HealthStatistics.FileValidator, fileSystemOptions.HealthStatistics.FolderValidator);
        }

        public static IDictionary<string, object> AppendContract(this IDictionary<string, object> options, IDisposable unlockContract)
        {
            if (unlockContract is not IEnumerable<KeyValuePair<string, object>> enumerable)
                return options;

            foreach (var item in enumerable)
                options.TryAdd(item.Key, item.Value);

            return options;
        }
    }
}
