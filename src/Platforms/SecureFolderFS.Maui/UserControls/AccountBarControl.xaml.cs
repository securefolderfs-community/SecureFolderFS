using SecureFolderFS.Sdk.Ftp.ViewModels;
using SecureFolderFS.Sdk.GoogleDrive.ViewModels;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class AccountBarControl : ContentView
    {
        public AccountBarControl()
        {
            InitializeComponent();
        }
        
        public IDisposable? AccountViewModel
        {
            get => (IDisposable?)GetValue(AccountViewModelProperty);
            set => SetValue(AccountViewModelProperty, value);
        }
        public static readonly BindableProperty AccountViewModelProperty =
            BindableProperty.Create(nameof(AccountViewModel), typeof(IDisposable), typeof(AccountBarControl),
                propertyChanged: static (bindable, value, newValue) =>
                {
                    if (bindable is not AccountBarControl accountBarControl)
                        return;

                    switch (newValue)
                    {
                        case GDriveAccountViewModel gDriveViewModel:
                        {
                            accountBarControl.UserAvatar.IsVisible = gDriveViewModel.UserPhotoUri is not null;
                            accountBarControl.UserAvatar.ImageSource = gDriveViewModel.UserPhotoUri is null ? ImageSource.FromStream(static () => Stream.Null) : ImageSource.FromUri(gDriveViewModel.UserPhotoUri);
                            accountBarControl.UserTitle.Text = gDriveViewModel.UserDisplayName ?? string.Empty;
                            accountBarControl.UserSubtitle.Text = gDriveViewModel.UserEmail ?? string.Empty;
                            break;
                        }

                        case FtpAccountViewModel ftpViewModel:
                        {
                            accountBarControl.UserAvatar.IsVisible = false;
                            accountBarControl.UserTitle.Text = ftpViewModel.UserName ?? string.Empty;
                            accountBarControl.UserSubtitle.Text = ftpViewModel.Address ?? string.Empty;
                            break;
                        }
                    }
                });
    }
}

