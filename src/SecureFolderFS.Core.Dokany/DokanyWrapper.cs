﻿using DokanNet;
using DokanNet.Logging;
using SecureFolderFS.Core.Dokany.Callbacks;

namespace SecureFolderFS.Core.Dokany
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
                    opt.Options = DokanOptions.CaseSensitive;
                    opt.UNCName = FileSystem.Constants.UNC_NAME;
                    opt.MountPoint = mountPoint;
                });

            _dokanInstance = dokanBuilder.Build(_dokanCallbacks);
        }

        public bool CloseFileSystem()
        {
            _dokanInstance?.Dispose();
            _dokanCallbacks.Dispose();

            return true;
        }
    }
}
