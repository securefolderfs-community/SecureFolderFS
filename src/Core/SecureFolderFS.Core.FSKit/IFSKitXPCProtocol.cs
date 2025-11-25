using System;
using Foundation;

namespace SecureFolderFS.Core.FSKit
{
    [Protocol]
    public interface IFSKitXPCProtocol
    {
        [Export("sendMessage:withReply:")]
        void SendMessage(string message, Action<string> reply);
    }
}