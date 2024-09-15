using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Popups
{
    public partial class PreviewRecoveryPopup : Popup, IOverlayControl
    {
        public PreviewRecoveryPopup()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        public Task<IResult> ShowAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public Task HideAsync()
        {
            throw new NotImplementedException();
        }
    }
}

