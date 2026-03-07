using CommunityToolkit.Maui.Behaviors;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources;

namespace SecureFolderFS.Maui.Views.Modals.Wizard
{
    public partial class AccountListSourceWizardPage : BaseModalPage
    {
        public AccountListSourceWizardPage(AccountSourceWizardViewModel viewModel, WizardOverlayViewModel overlayViewModel)
        {
            ViewModel = viewModel;
            OverlayViewModel = overlayViewModel;
            BindingContext = this;

            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnAppearing()
        {
            OverlayViewModel.CurrentViewModel = ViewModel;
            base.OnAppearing();
        }
        
        private async void RemoveAccount_Clicked(object? sender, EventArgs e)
        {
            if (sender is not SwipeItem { CommandParameter: AccountViewModel accountViewModel })
                return;
            
            await ViewModel.RemoveAccountCommand.ExecuteAsync(accountViewModel);
        }
        
        private async void TapGestureRecognizer_Tapped(object? sender, TappedEventArgs e)
        {
            if (sender is not View { BindingContext: AccountViewModel accountViewModel })
                return;
            
            await ViewModel.SelectAccountCommand.ExecuteAsync(accountViewModel);
        }
        
        private async void OptionsControl_Clicked(object? sender, EventArgs e)
        {
            if (sender is not TouchBehavior { CommandParameter: AccountViewModel accountViewModel })
                return;
            
            await ViewModel.SelectAccountCommand.ExecuteAsync(accountViewModel);
        }
        
        public AccountSourceWizardViewModel ViewModel
        {
            get => (AccountSourceWizardViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly BindableProperty ViewModelProperty =
            BindableProperty.Create(nameof(ViewModel), typeof(AccountSourceWizardViewModel), typeof(AccountListSourceWizardPage));
        
        public WizardOverlayViewModel OverlayViewModel
        {
            get => (WizardOverlayViewModel)GetValue(OverlayViewModelProperty);
            set => SetValue(OverlayViewModelProperty, value);
        }
        public static readonly BindableProperty OverlayViewModelProperty =
            BindableProperty.Create(nameof(OverlayViewModel), typeof(WizardOverlayViewModel), typeof(AccountListSourceWizardPage));
    }
}
