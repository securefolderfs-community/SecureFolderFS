using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace SecureFolderFS.Uno.Extensions
{
    public static class VisualTreeHelperExtensions
    {
        public static T? GetParent<T>(this DependencyObject obj)
        {
            return (T?)GetParent(obj, typeof(T));
        }

        public static object? GetParent(DependencyObject? obj, Type type)
        {
            while (true)
            {
                if (obj is null)
                    return null;

                var parent = VisualTreeHelper.GetParent(obj);
                if (parent is null)
                    return null;

                if (type.IsInstanceOfType(parent))
                    return parent;

                obj = parent;
            }
        }
    }
}
