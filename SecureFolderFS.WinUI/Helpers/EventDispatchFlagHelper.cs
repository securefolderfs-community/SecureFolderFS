using SecureFolderFS.Backend.Utils;
using System;

namespace SecureFolderFS.WinUI.Helpers
{
    internal sealed class EventDispatchFlagHelper : IEventDispatchFlag
    {
        private readonly Action _flagCallback;

        public EventDispatchFlagHelper(Action flagCallback)
        {
            this._flagCallback = flagCallback;
        }

        public void NoForwarding()
        {
            _flagCallback();
        }
    }
}
