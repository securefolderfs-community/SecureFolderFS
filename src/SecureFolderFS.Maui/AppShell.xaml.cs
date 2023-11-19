namespace SecureFolderFS.Maui
{
    public partial class AppShell : Shell
    {
        public bool IsNavbarVisible { get; } = DeviceInfo.Current.Platform != DevicePlatform.WinUI;

        public AppShell()
        {
            InitializeComponent();
        }
    }
}
