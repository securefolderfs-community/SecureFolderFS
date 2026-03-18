using System.ComponentModel;
using Microsoft.UI.Xaml.Markup;

#if !WINDOWS
using System;
#endif

namespace SecureFolderFS.Uno.Extensions
{
    /// <summary>
    /// A markup extension that provides platform-specific values for inline XAML usage.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class OnPlatformExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the value for Windows platform.
        /// </summary>
        public object? Windows { get; set; }

        /// <summary>
        /// Gets or sets the value for MacOS platform.
        /// </summary>
        public object? MacOS { get; set; }

        /// <summary>
        /// Gets or sets the value for Linux platform.
        /// </summary>
        public object? Linux { get; set; }

        /// <summary>
        /// Gets or sets the default value used when the platform-specific value is not set.
        /// </summary>
        public object? Default { get; set; }

        /// <inheritdoc/>
        protected override object? ProvideValue()
        {
#if WINDOWS
            return Windows ?? Default;
#else
            if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
                return MacOS ?? Default;

            if (OperatingSystem.IsLinux())
                return Linux ?? Default;

            return Default;
#endif
        }
    }
}
