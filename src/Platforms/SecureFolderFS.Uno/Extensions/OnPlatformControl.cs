using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace SecureFolderFS.Uno.Extensions
{
    /// <summary>
    /// Specifies a platform for use with <see cref="On"/> values.
    /// </summary>
    public enum Platform
    {
        /// <summary>
        /// Windows platform.
        /// </summary>
        Windows,

        /// <summary>
        /// MacOS platform.
        /// </summary>
        MacOS,

        /// <summary>
        /// Linux platform.
        /// </summary>
        Linux,
        
        /// <summary>
        /// The default platform when no specific platform matches.
        /// </summary>
        Default
    }

    /// <summary>
    /// Represents a platform-specific value for use with <see cref="OnPlatform"/>.
    /// </summary>
    [ContentProperty(Name = nameof(Value))]
    public sealed class On : FrameworkElement
    {
        /// <summary>
        /// Gets or sets the platform this value applies to.
        /// </summary>
        public Platform Platform { get; set; }

        /// <summary>
        /// Gets or sets the value for the specified platform.
        /// </summary>
        public object? Value { get; set; }
    }

    /// <summary>
    /// A control that provides platform-specific values.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ContentProperty(Name = nameof(Platforms))]
    public sealed class OnPlatform : ContentControl
    {
        /// <summary>
        /// Gets the collection of platform-specific values.
        /// </summary>
        public IList<On> Platforms { get; } = new List<On>();

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            Content ??= GetPlatformValue();
            base.OnApplyTemplate();
        }

        private object? GetPlatformValue()
        {
            var currentPlatform = GetCurrentPlatform();
            var platformValue = Platforms.FirstOrDefault(p => (p as On)!.Platform == currentPlatform)
                                ?? Platforms.FirstOrDefault(p => (p as On)!.Platform == Platform.Default);

            return platformValue?.Value;
        }

        private static Platform GetCurrentPlatform()
        {
#if WINDOWS
            return Platform.Windows;
#else
            if (System.OperatingSystem.IsMacCatalyst() || System.OperatingSystem.IsMacOS())
                return Platform.MacOS;

            if (System.OperatingSystem.IsLinux())
                return Platform.Linux;

            return Platform.Default; // Fallback
#endif
        }
    }
}

