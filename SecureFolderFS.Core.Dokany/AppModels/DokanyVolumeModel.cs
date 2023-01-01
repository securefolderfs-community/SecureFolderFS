using DokanNet;

namespace SecureFolderFS.Core.Dokany.AppModels
{
    internal sealed class DokanyVolumeModel
    {
        public required string VolumeName { get; init; }

        public required string FileSystemName { get; init; }

        public required uint MaximumComponentLength { get; init; }

        public required FileSystemFeatures FileSystemFeatures { get; init; }
    }
}
