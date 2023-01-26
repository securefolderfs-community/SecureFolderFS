namespace SecureFolderFS.UI.UserControls.BreadcrumbBar
{
    public sealed class OrderedBreadcrumbBarItem
    {
        public string Name { get; }

        public bool IsLeading { get; }

        public OrderedBreadcrumbBarItem(string name, bool isLeading)
        {
            Name = name;
            IsLeading = isLeading;
        }
    }
}
