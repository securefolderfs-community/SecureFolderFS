using System;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.WinUI.Helpers
{
    /// <inheritdoc cref="IEventDispatchFlag"/>
    public sealed class EventDispatchFlagHelper : IEventDispatchFlag
    {
        private readonly Action _flagCallback;

        public EventDispatchFlagHelper(Action flagCallback)
        {
            _flagCallback = flagCallback;
        }

        /// <inheritdoc/>
        public void NoForwarding()
        {
            _flagCallback();
        }
    }
}
