using SecureFolderFS.Core.FileSystem.Enums;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem
{
    // TODO: Needs docs
    public interface IFileSystemAvailabilityChecker
    {
        Task<FileSystemAvailabilityType> DetermineAvailabilityAsync();
    }
}
