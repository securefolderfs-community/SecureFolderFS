namespace SecureFolderFS.Backend.ViewModels.Dashboard.Navigation
{
    public interface IDashboardNavigationItemSource
    {
        int Index { get; }

        Action<DashboardNavigationItemViewModel?> NavigationAction { get; }

        string SectionName { get; }
    }
}
