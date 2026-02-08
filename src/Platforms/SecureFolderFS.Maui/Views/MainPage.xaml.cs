using SecureFolderFS.Maui.ServiceImplementation;
using SecureFolderFS.Maui.UserControls.Navigation;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.VaultList;
using SecureFolderFS.Sdk.ViewModels.Views.Host;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.UI.Helpers;

namespace SecureFolderFS.Maui.Views
{
    public partial class MainPage : ContentPage
    {
        public MainHostViewModel? ViewModel
        {
            get
            {
                if (field is not null)
                    return field;

                var mainViewModel = Shell.Current.TryCast<AppShell>()!.MainViewModel;
                return field ??= new(mainViewModel.VaultListViewModel, mainViewModel.VaultCollectionModel);
            }
        }

        public static MainPage? Instance { get; private set; }

        public MainPage()
        {
            Instance = this;
            BindingContext = this;
            _ = ViewModel?.InitAsync();

            InitializeComponent();
        }

        private async Task ItemTappedAsync(VaultListItemViewModel itemViewModel, View? view)
        {
            if (ViewModel is null)
                return;
            
            if (view is not null)
                view.IsEnabled = false;

            try
            {
                ViewModel.NavigationService.SetupNavigation(ShellNavigationControl.Instance);
                var target = ViewModel.NavigationService.Views.FirstOrDefault(x => (x as IVaultViewContext)?.VaultViewModel.VaultModel.Equals(itemViewModel.VaultViewModel.VaultModel) ?? false);
                if (target is null)
                {
                    var vaultLoginViewModel = new VaultLoginViewModel(itemViewModel.VaultViewModel, ViewModel.NavigationService);
                    _ = vaultLoginViewModel.InitAsync();
                    target = vaultLoginViewModel;
                }

                // Navigate
                await ViewModel.NavigationService.NavigateAsync(target);
            }
            finally
            {
                if (view is not null)
                    view.IsEnabled = true;
            }
        }

        private async void ItemTapRecognizer_Tapped(object? sender, TappedEventArgs e)
        {
            if (sender is not View { BindingContext: VaultListItemViewModel itemViewModel } view)
                return;

            await ItemTappedAsync(itemViewModel, view);
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            if (ViewModel is null)
                return;
            
            // Set the current starting view
            if (ViewModel.NavigationService.CurrentView is null && ViewModel.NavigationService is MauiNavigationService navigationService)
                navigationService.SetCurrentViewInternal(ViewModel);

#if IOS
            if (ToolbarItems.Count == 2)
                ToolbarItems.Insert(0, new()
                {
                    Command = ViewModel.VaultListViewModel.AddNewVaultCommand,
                    Order = ToolbarItemOrder.Secondary,
                    Text = "NewVault".ToLocalized()
                });
#endif
        }
    }
}
