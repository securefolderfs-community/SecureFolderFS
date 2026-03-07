using SecureFolderFS.Sdk.Dropbox.ViewModels;
using SecureFolderFS.Sdk.Ftp.ViewModels;
using SecureFolderFS.Sdk.GoogleDrive.ViewModels;
using SecureFolderFS.Sdk.WebDavClient.ViewModels;

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
                propertyChanged: static (bindable, _, newValue) =>
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

                        case DropboxAccountViewModel dropboxViewModel:
                        {
                            accountBarControl.UserAvatar.IsVisible = dropboxViewModel.UserPhotoUri is not null;
                            accountBarControl.UserAvatar.ImageSource = dropboxViewModel.UserPhotoUri is null ? ImageSource.FromStream(static () => Stream.Null) : ImageSource.FromUri(dropboxViewModel.UserPhotoUri);
                            accountBarControl.UserTitle.Text = dropboxViewModel.UserDisplayName ?? string.Empty;
                            accountBarControl.UserSubtitle.Text = dropboxViewModel.UserEmail ?? string.Empty;
                            break;
                        }

                        case FtpAccountViewModel ftpViewModel:
                        {
                            accountBarControl.UserAvatar.IsVisible = false;
                            accountBarControl.UserTitle.Text = ftpViewModel.UserName ?? string.Empty;
                            accountBarControl.UserSubtitle.Text = ftpViewModel.Address ?? string.Empty;
                            break;
                        }
                        
                        case WebDavClientAccountViewModel davClientAccountViewModel:
                        {
                            accountBarControl.UserAvatar.IsVisible = false;
                            accountBarControl.UserTitle.Text = davClientAccountViewModel.UserName ?? string.Empty;
                            accountBarControl.UserSubtitle.Text = davClientAccountViewModel.Address ?? string.Empty;
                            break;
                        }
                    }
                });
    }
}

