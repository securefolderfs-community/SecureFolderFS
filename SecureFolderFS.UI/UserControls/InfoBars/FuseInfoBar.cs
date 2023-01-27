using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.UI.UserControls.InfoBars
{
    public sealed class FuseInfoBar : InfoBarViewModel
    {
        public FuseInfoBar()
        {
            Message = "FUSE is experimental. You may encounter bugs and stability issues. We recommend backing up your data before using FUSE.";
        }
    }
}