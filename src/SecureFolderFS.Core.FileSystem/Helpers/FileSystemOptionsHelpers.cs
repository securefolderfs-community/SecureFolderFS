using SecureFolderFS.Core.FileSystem.Validators;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Helpers
{
    public static class FileSystemOptionsHelpers
    {
        public static void SetupValidators<TOptions>(this TOptions fileSystemOptions, FileSystemSpecifics specifics)
            where TOptions : FileSystemOptions
        {
            fileSystemOptions.HealthStatistics.FileValidator ??= new FileValidator(specifics);
            fileSystemOptions.HealthStatistics.FolderValidator ??= new FolderValidator(specifics);
            fileSystemOptions.HealthStatistics.StructureValidator ??= new StructureValidator(specifics, fileSystemOptions.HealthStatistics.FileValidator, fileSystemOptions.HealthStatistics.FolderValidator);
        }
    }
}
