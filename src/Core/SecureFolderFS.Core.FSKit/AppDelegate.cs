using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using AppKit;

namespace SecureFolderFS.Core.FSKit;

[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate
{
    private FSKitIPCServer? _ipcServer;

    public override void DidFinishLaunching(NSNotification notification)
    {
        // Start the Unix domain socket IPC server
        _ipcServer = new FSKitIPCServer();

        Task.Run(async () =>
        {
            try
            {
                await _ipcServer.StartAsync();
                Console.WriteLine("FSKit IPC server started successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start IPC server: {ex.Message}");
            }
        });
    }

    public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication sender)
    {
        _ipcServer?.Stop();
        return NSApplicationTerminateReply.Now;
    }
}