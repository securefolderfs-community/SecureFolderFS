using DokanNet;
using DokanNet.Logging;
using SecureFolderFS.Core.FileSystem.Enums;

namespace SecureFolderFS.Core.Dokany.Callbacks
{
    internal sealed class DokanyWrapper
    {
        private readonly BaseDokanCallbacks _dokanCallbacks;
        private Dokan? _dokan;
        private DokanInstance? _dokanInstance;

        public DokanyWrapper(BaseDokanCallbacks dokanCallbacks)
        {
            _dokanCallbacks = dokanCallbacks;
        }

        public void StartFileSystem(string mountPoint)
        {
            _dokan = new(new NullLogger());
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
            _dokan?.Dispose();

            return true;
        }
    }
}
