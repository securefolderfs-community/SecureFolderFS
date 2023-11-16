using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.UI.UserControls.InfoBars
{
    public sealed class WebDavInfoBar : InfoBarViewModel
    {
        public WebDavInfoBar()
        {
            Message = "WebDav is experimental. You may encounter bugs and stability issues. We recommend backing up your data before using WebDav.";
        }
    }
}
