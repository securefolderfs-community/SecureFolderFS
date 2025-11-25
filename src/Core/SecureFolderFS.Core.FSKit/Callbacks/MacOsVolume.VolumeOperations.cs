using System;
using Foundation;
using FSKit;

namespace SecureFolderFS.Core.FSKit.Callbacks
{
    internal sealed partial class MacOsVolume : IFSVolumeOperations
    {
        /// <inheritdoc/>
        public FSVolumeSupportedCapabilities SupportedVolumeCapabilities { get; }

        /// <inheritdoc/>
        public FSStatFSResult VolumeStatistics { get; }

        /// <inheritdoc/>
        public void Activate(FSTaskOptions options, FSVolumeOperationsActivateHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Deactivate(FSDeactivateOptions options, FSVolumeOperationsDeactivateHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Unmount(Action reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Synchronize(FSSyncFlags flags, FSVolumeOperationsSynchronizeHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void GetAttributes(FSItemGetAttributesRequest desiredAttributes, FSItem item,
            FSVolumeOperationsAttributesHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetAttributes(FSItemSetAttributesRequest newAttributes, FSItem item, FSVolumeOperationsAttributesHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void LookupItem(FSFileName name, FSItem directory, FSVolumeOperationsLookupItemHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Reclaim(FSItem item, FSVolumeOperationsReclaimHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ReadSymbolicLink(FSItem item, FSVolumeOperationsReadSymbolicLinkHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void CreateItem(FSFileName name, FSItemType type, FSItem directory, FSItemSetAttributesRequest newAttributes,
            FSVolumeOperationsCreateItemHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void CreateSymbolicLink(FSFileName name, FSItem directory, FSItemSetAttributesRequest newAttributes,
            FSFileName contents, FSVolumeOperationsCreateItemHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void CreateLink(FSItem item, FSFileName name, FSItem directory, FSVolumeOperationsCreateLinkHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveItem(FSItem item, FSFileName name, FSItem directory, FSVolumeOperationsRemoveItemHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RenameItem(FSItem item, FSItem sourceDirectory, FSFileName sourceName, FSFileName destinationName,
            FSItem destinationDirectory, FSItem? overItem, FSVolumeOperationsRenameItemHandler reply)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void EnumerateDirectory(FSItem directory, ulong startingAt, ulong verifier, FSItemGetAttributesRequest? attributes,
            FSDirectoryEntryPacker packer, FSVolumeOperationsEnumerateDirectoryHandler reply)
        {
            throw new NotImplementedException();
        }
    }
}