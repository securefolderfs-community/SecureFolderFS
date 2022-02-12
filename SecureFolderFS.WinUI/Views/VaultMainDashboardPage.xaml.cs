using System;
using System.Runtime.CompilerServices;
using System.Threading;
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
        private readonly SemaphoreSlim _graphClickSemaphore;

        public VaultMainDashboardPageViewModel ViewModel
        {
            get => (VaultMainDashboardPageViewModel)DataContext;
            set => DataContext = value;
        }

        public VaultMainDashboardPage()
        {
            this.InitializeComponent();

            _graphClickSemaphore = new(1, 1);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VaultMainDashboardPageViewModel viewModel)
            {
                this.ViewModel = viewModel;

                viewModel.ReadGraphViewModel.GraphDisposable = (IDisposable)ReadGraph;
                viewModel.WriteGraphViewModel.GraphDisposable = (IDisposable)WriteGraph;

                RestoreGraphsState();
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _graphClickSemaphore.Dispose();
            ViewModel.Cleanup();
            base.OnNavigatingFrom(e);
        }

        private void RestoreGraphsState()
        {
            if (ViewModel.ReadGraphViewModel.IsExtended)
            {
                WriteGraph.Visibility = Visibility.Collapsed;
                HideColumn(WriteColumn);
                GraphsGrid.ColumnSpacing = 0;
            }
            else if (ViewModel.WriteGraphViewModel.IsExtended)
            {
                ReadGraph.Visibility = Visibility.Collapsed;
                HideColumn(ReadColumn);
                GraphsGrid.ColumnSpacing = 0;
            }
        }

        private async Task StartGraphHideStoryboard(UIElement element, [CallerArgumentExpression("element")] string? name = null)
        {
            Storyboard.SetTargetName(GraphHideStoryboard.Children[0], name);
            Storyboard.SetTargetName(GraphHideStoryboard.Children[1], name); 
            await GraphHideStoryboard.BeginAsync();
            element.Visibility = Visibility.Collapsed;
            GraphHideStoryboard.Stop();
        }

        private async Task StartGraphExtendStoryboard(FrameworkElement element, [CallerArgumentExpression("element")] string? name = null)
        {
            Storyboard.SetTargetName(GraphExtendStoryboard.Children[0], name);
            await GraphExtendStoryboard.BeginAsync();
            GraphExtendStoryboard.Stop();
        }

        private async Task StartGraphRetractStoryboard(UIElement element, [CallerArgumentExpression("element")] string? name = null)
        {
            Storyboard.SetTargetName(GraphRetractStoryboard.Children[0], name);
            await GraphRetractStoryboard.BeginAsync();
            GraphRetractStoryboard.Stop();
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
            column.Width = new(0, GridUnitType.Star);
        }

        private void RestoreColumn(ColumnDefinition column)
        {
            GraphsGrid.ColumnSpacing = 8;
            column.Width = new(1, GridUnitType.Star);
        }

        private async void ReadGraph_Click(object sender, RoutedEventArgs e)
        {
            if (!await _graphClickSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                if (!ViewModel.ReadGraphViewModel.IsExtended)
                {
                    await StartGraphHideStoryboard(WriteGraph);
                    GraphsGrid.ColumnSpacing = 0;
                    HideColumn(WriteColumn);
                    await StartGraphExtendStoryboard(ReadGraph);
                }
                else
                {
                    RestoreColumn(WriteColumn);
                    await StartGraphRetractStoryboard(ReadGraph);
                    await StartGraphRestoreStoryboard(WriteGraph);
                }

                ViewModel.ReadGraphViewModel.IsExtended = !ViewModel.ReadGraphViewModel.IsExtended;
            }
            finally
            {
                _graphClickSemaphore.Release();
            }
        }

        private async void WriteGraph_Click(object sender, RoutedEventArgs e)
        {
            if (!await _graphClickSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                if (!ViewModel.WriteGraphViewModel.IsExtended)
                {
                    await StartGraphHideStoryboard(ReadGraph);
                    GraphsGrid.ColumnSpacing = 0;
                    HideColumn(ReadColumn);
                    await StartGraphExtendStoryboard(WriteGraph);
                }
                else
                {
                    RestoreColumn(ReadColumn);
                    await StartGraphRetractStoryboard(WriteGraph);
                    await StartGraphRestoreStoryboard(ReadGraph);
                }

                ViewModel.WriteGraphViewModel.IsExtended = !ViewModel.WriteGraphViewModel.IsExtended;
            }
            finally
            {
                _graphClickSemaphore.Release();
            }
        }
    }
}
