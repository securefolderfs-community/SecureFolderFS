using System;
using Foundation;

namespace SecureFolderFS.Core.FSKit
{
    public class FSKitXPCService : NSObject, IFSKitXPCProtocol
    {
        public void SendMessage(string message, Action<string> reply)
        {
            Console.WriteLine($"FSKit XPC Service received: {message}");
            var response = $"Echo from FSKit: {message}";

            reply(response);
        }
    }
}