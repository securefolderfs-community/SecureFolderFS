using System;
using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.Shared.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.Views.Vault
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [INotifyPropertyChanged]
    public sealed partial class VaultOverviewPage : Page, IDisposable
    {
        public VaultOverviewViewModel? ViewModel
        {
            get => DataContext.TryCast<VaultOverviewViewModel>();
            set { DataContext = value; OnPropertyChanged(); }
        }

        public VaultOverviewPage()
        {
            InitializeComponent();
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultOverviewViewModel viewModel)
                ViewModel = viewModel;

            base.OnNavigatedTo(e);
        }

        /// <inheritdoc/>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Dispose();
            base.OnNavigatingFrom(e);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var item in (WidgetsList.ItemsSource as IEnumerable))
            {
                (item as IDisposable)?.Dispose();
            }
        }
    }
}
