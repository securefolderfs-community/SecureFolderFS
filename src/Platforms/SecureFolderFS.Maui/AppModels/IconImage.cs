using MauiIcons.Core;
using IImage = SecureFolderFS.Shared.ComponentModel.IImage;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="IImage"/>
    internal sealed class IconImage : IImage
    {
        /// <summary>
        /// Gets the icon of type <see cref="MauiIcon"/> to retrieve the Maui icon representation.
        /// </summary>
        public MauiIcon MauiIcon { get; }

        public IconImage(MauiIcon mauiIcon)
        {
            MauiIcon = mauiIcon;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
