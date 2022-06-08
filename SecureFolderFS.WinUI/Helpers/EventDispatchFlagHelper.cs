using System;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.WinUI.Helpers
{
    internal sealed class EventDispatchFlagHelper : IEventDispatchFlag
    {
        private readonly Action _flagCallback;

        public EventDispatchFlagHelper(Action flagCallback)
        {
            _flagCallback = flagCallback;
        }

        public void NoForwarding()
        {
            _flagCallback();
        }
    }
}
