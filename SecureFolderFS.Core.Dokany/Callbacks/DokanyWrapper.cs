using DokanNet;
using DokanNet.Logging;
using SecureFolderFS.Core.FileSystem.Enums;

namespace SecureFolderFS.Core.Dokany.Callbacks
{
    internal sealed class DokanyWrapper
    {
        private readonly Dokan _dokan;
        private readonly BaseDokanyCallbacks _dokanCallbacks;
        private DokanInstance? _dokanInstance;

        public DokanyWrapper(BaseDokanyCallbacks dokanCallbacks)
        {
            _dokan = new(new NullLogger());
            _dokanCallbacks = dokanCallbacks;
        }

        public void StartFileSystem(string mountPoint)
        {
            var dokanBuilder = new DokanInstanceBuilder(_dokan)
                .ConfigureOptions(opt =>
                {
                    opt.Options = DokanOptions.CaseSensitive | DokanOptions.FixedDrive;
                    opt.UNCName = FileSystem.Constants.UNC_NAME;
                    opt.MountPoint = mountPoint;
                });

            _dokanInstance = dokanBuilder.Build(_dokanCallbacks as IDokanOperationsUnsafe);
        }

        public bool CloseFileSystem(FileSystemCloseMethod closeMethod)
        {
            _ = closeMethod; // TODO: Implement close method
            _dokanInstance?.Dispose();

            return true;
        }
    }
}
