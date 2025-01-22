using System;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class SkiaSystemService : ISystemService
    {
        private EventHandler? _desktopLocked;

        /// <inheritdoc/>
        public event EventHandler? DesktopLocked // TODO: Implement on linux
        {
            add => _desktopLocked += value;
            remove => _desktopLocked -= value;
        }
    }
}
