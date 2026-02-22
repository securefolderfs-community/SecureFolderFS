namespace SecureFolderFS.Maui.Extensions
{
    internal static class SizingExtensions
    {
        public static Rect GetBoundsRelativeTo(this VisualElement view, VisualElement relativeTo)
        {
            var bounds = view.Bounds;
            var parent = view.Parent as VisualElement;

            while (parent != null && parent != relativeTo)
            {
                bounds.X += parent.X;
                bounds.Y += parent.Y;
                parent = parent.Parent as VisualElement;
            }

            return bounds;
        }
    }
}
