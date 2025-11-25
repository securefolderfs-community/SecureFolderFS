using System;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using SecureFolderFS.Core.FSKit.Ipc;

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
        _ipcServer?.StopAsync().GetAwaiter().GetResult();
        return NSApplicationTerminateReply.Now;
    }
}