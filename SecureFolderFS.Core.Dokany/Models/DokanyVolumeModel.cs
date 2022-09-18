using DokanNet;
using SecureFolderFS.Core.FileSystem.Models;

namespace SecureFolderFS.Core.Dokany.Models
{
    internal sealed class DokanyVolumeModel : VolumeModel
    {
        // TODO: Add required modifier
        public string FileSystemName { get; init; }

        public uint MaximumComponentLength { get; init; }

        public FileSystemFeatures FileSystemFeatures { get; init; }
    }
}
