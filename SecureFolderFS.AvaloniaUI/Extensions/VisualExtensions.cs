using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Linq;

namespace SecureFolderFS.AvaloniaUI.Extensions
{
    internal static class VisualExtensions
    {
        /// <exception cref="ArgumentException">The specified control doesn't have the requested transform.</exception>
        public static TTransform GetTransform<TTransform>(this IVisual control)
            where TTransform : Transform
        {
            if (control.RenderTransform is TTransform targetTransform)
                return targetTransform;

            TTransform? transform = null;
            if (control.RenderTransform is TransformGroup transformGroup)
                transform = (TTransform?)transformGroup.Children?.FirstOrDefault(x => x is TTransform);

            if (transform is null)
                throw new ArgumentException($"Target must have a {typeof(TTransform)}.");

            return transform;
        }
    }
}