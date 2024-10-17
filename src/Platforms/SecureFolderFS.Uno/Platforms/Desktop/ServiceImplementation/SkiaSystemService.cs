using System;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="ISystemService"/>
    internal sealed class SkiaSystemService : BaseSystemService
    {
        /// <inheritdoc/>
        protected override void AttachEvent(ref EventHandler? handler, EventHandler? value)
        {
            if (!OperatingSystem.IsLinux())
                return;
            
            //throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override void DetachEvent(ref EventHandler? handler, EventHandler? value)
        {
            if (!OperatingSystem.IsLinux())
                return;
            
            //throw new NotImplementedException();
        }
    }
}
