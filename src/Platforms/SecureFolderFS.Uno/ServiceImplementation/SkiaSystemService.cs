using System;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed partial class SkiaSystemService : ISystemService
    {
        private EventHandler? _desktopLocked;

        /// <inheritdoc/>
        public event EventHandler? DesktopLocked
        {
            add => AttachEvent(ref _desktopLocked, value);
            remove => DetachEvent(ref _desktopLocked, value);
        }

        partial void AttachEvent(ref EventHandler? handler, EventHandler? value);

        partial void DetachEvent(ref EventHandler? handler, EventHandler? value);
    }
}
