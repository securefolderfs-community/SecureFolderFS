using System.ComponentModel;
using Microsoft.UI.Xaml.Markup;

namespace SecureFolderFS.Uno.Extensions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class OnPlatform : MarkupExtension
    {
        public object? Windows { get; set; }
        
        public object? Skia { get; set; }
        
        /// <inheritdoc/>
        protected override object? ProvideValue()
        {
#if HAS_UNO_SKIA || HAS_UNO
            return Skia;
#elif WINDOWS
            return Windows;
#else
            throw new System.PlatformNotSupportedException();
#endif
        }
    }
}
