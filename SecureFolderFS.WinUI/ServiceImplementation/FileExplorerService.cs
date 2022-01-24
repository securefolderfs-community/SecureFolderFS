using System;
using System.Threading.Tasks;
using Windows.System;
using SecureFolderFS.Backend.Services;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    internal sealed class FileExplorerService : IFileExplorerService
    {
        public async Task OpenPathInFileExplorerAsync(string path)
        {
            await Launcher.LaunchFolderPathAsync(path);
        }
    }
}
