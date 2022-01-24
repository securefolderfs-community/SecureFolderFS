using DokanNet;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.FileSystem.OpenHandles;
using SecureFolderFS.Core.Helpers;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class GetVolumeInformationCallback : BaseDokanOperationsCallback, IGetVolumeInformationCallback
    {
        private readonly MountVolumeDataModel _mountVolumeDataModel;

        private readonly FileSystemFeatures _dokanFileSystemFeatures;

        public GetVolumeInformationCallback(MountVolumeDataModel mountVolumeDataModel, HandlesCollection handles)
            : base(handles)
        {
            this._mountVolumeDataModel = mountVolumeDataModel;

            this._dokanFileSystemFeatures = mountVolumeDataModel.FileSystemFlags.ToDokanFileSystemFlags();
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            volumeLabel = _mountVolumeDataModel.VolumeName;
            fileSystemName = _mountVolumeDataModel.FileSystemName;
            maximumComponentLength = _mountVolumeDataModel.MaximumComponentLength;
            features = _dokanFileSystemFeatures;

            return DokanResult.Success;
        }
    }
}
