﻿using SecureFolderFS.Shared.ComponentModel;
using System;

namespace SecureFolderFS.UI.Helpers
{
    /// <inheritdoc cref="IEventDispatch"/>
    public sealed class EventDispatchHelper : IEventDispatch
    {
        private readonly Action _flagCallback;

        public EventDispatchHelper(Action flagCallback)
        {
            _flagCallback = flagCallback;
        }

        /// <inheritdoc/>
        public void PreventForwarding()
        {
            _flagCallback();
        }
    }
}
