using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

#nullable enable

namespace SecureFolderFS.WinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VaultMainDashboardPage : Page
    {
        public VaultMainDashboardPageViewModel ViewModel
        {
            get => (VaultMainDashboardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultMainDashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultMainDashboardPageViewModel viewModel)
            {
                this.ViewModel = viewModel;

                viewModel.ReadGraphViewModel.GraphDisposable = (IDisposable)ReadGraph;
                viewModel.WriteGraphViewModel.GraphDisposable = (IDisposable)WriteGraph;
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModel.Cleanup();
            base.OnNavigatingFrom(e);
        }

        private async void ReadGraph_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.ReadGraphViewModel.IsExtended)
            {
                await StartGraphHideStoryboard(WriteGraph);
                HideColumn(WriteColumn);
            }
            else
            {
                RestoreColumn(WriteColumn);
                await StartGraphRestoreStoryboard(WriteGraph);
            }

            ViewModel.ReadGraphViewModel.IsExtended = !ViewModel.ReadGraphViewModel.IsExtended;
        }

        private async void WriteGraph_Click(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.WriteGraphViewModel.IsExtended)
            {
                await StartGraphHideStoryboard(ReadGraph);
                HideColumn(ReadColumn);
            }
            else
            {
                RestoreColumn(ReadColumn);
                await StartGraphRestoreStoryboard(ReadGraph);
            }

            ViewModel.WriteGraphViewModel.IsExtended = !ViewModel.WriteGraphViewModel.IsExtended;
        }

        private async Task StartGraphHideStoryboard(UIElement element, [CallerArgumentExpression("element")] string? name = null)
        {
            Storyboard.SetTargetName(GraphHideStoryboard.Children[0], name);
            Storyboard.SetTargetName(GraphHideStoryboard.Children[1], name); 
            await GraphHideStoryboard.BeginAsync();
            element.Visibility = Visibility.Collapsed;
            GraphHideStoryboard.Stop();
        }

        private async Task StartGraphRestoreStoryboard(UIElement element, [CallerArgumentExpression("element")] string? name = null)
        {
            Storyboard.SetTargetName(GraphRestoreStoryboard.Children[0], name);
            Storyboard.SetTargetName(GraphRestoreStoryboard.Children[1], name); 
            element.Visibility = Visibility.Visible;
            await GraphRestoreStoryboard.BeginAsync(); 
            GraphRestoreStoryboard.Stop();
        }

        private void HideColumn(ColumnDefinition column)
        {
            GraphsGrid.ColumnSpacing = 0;
            column.Width = new(0, GridUnitType.Star);
        }

        private void RestoreColumn(ColumnDefinition column)
        {
            GraphsGrid.ColumnSpacing = 8;
            column.Width = new(1, GridUnitType.Star);
        }
    }
}
