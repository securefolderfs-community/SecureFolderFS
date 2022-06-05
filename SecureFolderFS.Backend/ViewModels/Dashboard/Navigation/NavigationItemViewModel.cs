namespace SecureFolderFS.Backend.ViewModels.Dashboard.Navigation
{
    public sealed class NavigationItemViewModel
    {
        public bool IsLeading { get; set; }

        public int Index { get; init; }

        public string? SectionName { get; init; }

        public Action<NavigationItemViewModel?>? NavigationAction { get; init; }
    }
}
