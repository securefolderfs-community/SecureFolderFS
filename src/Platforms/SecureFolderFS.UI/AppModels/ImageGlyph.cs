using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.AppModels
{
    public sealed class ImageGlyph(string glyph, string? fontFamily = null) : IImage
    {
        /// <summary>
        /// Gets the glyph string to display.
        /// </summary>
        public string Glyph { get; } = glyph;

        /// <summary>
        /// Gets the optional font family name of the glyph.
        /// </summary>
        public string? FontFamily { get; } = fontFamily;

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
