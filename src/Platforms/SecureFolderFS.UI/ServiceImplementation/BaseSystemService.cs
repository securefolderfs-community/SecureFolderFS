using System;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    public abstract class BaseSystemService : ISystemService
    {
        protected EventHandler? desktopLocked;

        /// <inheritdoc/>
        public event EventHandler? DesktopLocked
        {
            add => AttachEvent(ref desktopLocked, value);
            remove => DetachEvent(ref desktopLocked, value);
        }

        protected abstract void AttachEvent(ref EventHandler? handler, EventHandler? value);

        protected abstract void DetachEvent(ref EventHandler? handler, EventHandler? value);
    }
}
