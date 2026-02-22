using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

#if !HAS_UNO_SKIA
using CommunityToolkit.WinUI;
#endif

namespace SecureFolderFS.Uno.Extensions
{
    public static class VisualTreeHelperExtensions
    {
        public static FrameworkElement? GetContentControlRoot(this ContentControl contentControl)
        {
#if HAS_UNO_SKIA
            var templatedRoot = typeof(ContentControl)
                .GetProperty("TemplatedRoot", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(contentControl) as FrameworkElement;

            return templatedRoot is not ContentPresenter { ContentTemplateRoot: FrameworkElement contentRoot } ? null : contentRoot;
#else
            if (contentControl.ContentTemplateRoot is not { } contentTemplateRoot)
            {
                var presenter = contentControl.FindChild<ContentPresenter>();
                return presenter?.ContentTemplateRoot as FrameworkElement;
            }
            
            return contentTemplateRoot as FrameworkElement;
#endif
        }
        
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
