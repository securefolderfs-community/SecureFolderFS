using DokanNet;
using System.Collections.Generic;
using SecureFolderFS.Core.FileSystem.OpenHandles;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan.Callback.Implementation
{
    internal sealed class FindFilesCallback : BaseDokanOperationsCallback, IFindFilesCallback
    {
        private readonly IFindFilesWithPatternCallback _findFilesWithPatternCallback;

        public FindFilesCallback(IFindFilesWithPatternCallback findFilesWithPatternCallback, HandlesCollection handles)
            : base(handles)
        {
            _findFilesWithPatternCallback = findFilesWithPatternCallback;
        }

        public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
        {
            return _findFilesWithPatternCallback.FindFilesWithPattern(fileName, "*", out files, info);
        }
    }
}
