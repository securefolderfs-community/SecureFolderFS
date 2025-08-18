using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.UI.Helpers
{
    /// <inheritdoc cref="IEventDispatch"/>
    public sealed class EventDispatchHelper : IEventDispatch
    {
        private readonly Action _callback;

        public EventDispatchHelper(Action callback)
        {
            _callback = callback;
        }

        /// <inheritdoc/>
        public void PreventForwarding()
        {
            _callback();
        }
    }
}
