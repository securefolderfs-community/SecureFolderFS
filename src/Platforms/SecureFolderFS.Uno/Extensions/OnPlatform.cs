using System.ComponentModel;
using Microsoft.UI.Xaml.Markup;

#if !WINDOWS
using System;
#endif

namespace SecureFolderFS.Uno.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class OnPlatform : MarkupExtension
    {
        public object? Windows { get; set; }

        public object? MacOS { get; set; }

        public object? Linux { get; set; }

        /// <inheritdoc/>
        protected override object? ProvideValue()
        {
#if WINDOWS
            return Windows;
#else
            if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
                return MacOS;

            if (OperatingSystem.IsLinux())
                return Linux;

            throw new System.PlatformNotSupportedException();
#endif
        }
    }
}
