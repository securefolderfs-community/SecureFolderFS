using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.Utils;

namespace SecureFolderFS.Maui.Popups
{
    public partial class CredentialsPopup : Popup, IOverlayControl
    {
        public CredentialsPopup()
        {
            InitializeComponent();
        }
        
        /// <inheritdoc/>
        public async Task<IResult> ShowAsync()
        {
            if (ViewModel is null)
                return Shared.Helpers.Result.Failure(null);
            
            _ = await Shell.Current.CurrentPage.ShowPopupAsync(this);
            return Shared.Helpers.Result.Success;
        }

        /// <inheritdoc/>
        public void SetView(IViewable viewable)
        {
            ViewModel = (CredentialsOverlayViewModel)viewable;
        }
        
        /// <inheritdoc/>
        public Task HideAsync()
        {
            return Task.CompletedTask;
        }
        
        public CredentialsOverlayViewModel? ViewModel
        {
            get => (CredentialsOverlayViewModel?)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(CredentialsOverlayViewModel), typeof(CredentialsPopup), null);
    }
}

