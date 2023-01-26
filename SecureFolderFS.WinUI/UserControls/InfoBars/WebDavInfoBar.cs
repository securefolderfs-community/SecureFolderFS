using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.WinUI.UserControls.InfoBars
{
    internal sealed class WebDavInfoBar : InfoBarViewModel
    {
        public WebDavInfoBar()
        {
            Message = "WebDav is experimental. You may encounter bugs and stability issues. We recommend backing up your data before using WebDav.";
        }
    }
}
