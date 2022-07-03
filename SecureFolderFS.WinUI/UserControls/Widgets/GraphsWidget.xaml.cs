using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.Widgets
{
    public sealed partial class GraphsWidget : UserControl, IDisposable
    {
        private readonly SemaphoreSlim _graphClickSemaphore;

        public GraphsWidget()
        {
            InitializeComponent();

            _graphClickSemaphore = new(1, 1);
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
                if (!ReadGraphIsExtended)
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

                ReadGraphIsExtended = !ReadGraphIsExtended;
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
                if (!WriteGraphIsExtended)
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

                WriteGraphIsExtended = !WriteGraphIsExtended;
            }
            finally
            {
                _graphClickSemaphore.Release();
            }
        }

        public void RestoreGraphsState()
        {
            if (ReadGraphIsExtended)
            {
                WriteGraph.Visibility = Visibility.Collapsed;
                HideColumn(WriteColumn);
                GraphsGrid.ColumnSpacing = 0;
            }
            else if (WriteGraphIsExtended)
            {
                ReadGraph.Visibility = Visibility.Collapsed;
                HideColumn(ReadColumn);
                GraphsGrid.ColumnSpacing = 0;
            }
        }

        public void Dispose()
        {
            ReadGraph?.Dispose();
            WriteGraph?.Dispose();
        }

        public bool ReadGraphIsExtended
        {
            get => (bool)GetValue(ReadGraphIsExtendedProperty);
            set => SetValue(ReadGraphIsExtendedProperty, value);
        }
        public static readonly DependencyProperty ReadGraphIsExtendedProperty =
            DependencyProperty.Register(nameof(ReadGraphIsExtended), typeof(bool), typeof(GraphsWidget), new PropertyMetadata(false));


        public bool WriteGraphIsExtended
        {
            get => (bool)GetValue(WriteGraphIsExtendedProperty);
            set => SetValue(WriteGraphIsExtendedProperty, value);
        }
        public static readonly DependencyProperty WriteGraphIsExtendedProperty =
            DependencyProperty.Register(nameof(WriteGraphIsExtended), typeof(bool), typeof(GraphsWidget), new PropertyMetadata(false));


        public bool ReadGraphLoaded
        {
            get => (bool)GetValue(ReadGraphLoadedProperty);
            set => SetValue(ReadGraphLoadedProperty, value);
        }
        public static readonly DependencyProperty ReadGraphLoadedProperty =
            DependencyProperty.Register(nameof(ReadGraphLoaded), typeof(bool), typeof(GraphsWidget), new PropertyMetadata(false));


        public bool WriteGraphLoaded
        {
            get => (bool)GetValue(WriteGraphLoadedProperty);
            set => SetValue(WriteGraphLoadedProperty, value);
        }
        public static readonly DependencyProperty WriteGraphLoadedProperty =
            DependencyProperty.Register(nameof(WriteGraphLoaded), typeof(bool), typeof(GraphsWidget), new PropertyMetadata(false));


        // TODO: Make it independent of GraphPointViewModel. Maybe use IList?
        public ObservableCollection<GraphPointViewModel> ReadGraphData
        {
            get => (ObservableCollection<GraphPointViewModel>)GetValue(ReadGraphDataProperty);
            set => SetValue(ReadGraphDataProperty, value);
        }
        public static readonly DependencyProperty ReadGraphDataProperty =
            DependencyProperty.Register(nameof(ReadGraphData), typeof(ObservableCollection<GraphPointViewModel>), typeof(GraphsWidget), new PropertyMetadata(null));


        // TODO: Make it independent of GraphPointViewModel. Maybe use IList?
        public ObservableCollection<GraphPointViewModel> WriteGraphData
        {
            get => (ObservableCollection<GraphPointViewModel>)GetValue(WriteGraphDataProperty);
            set => SetValue(WriteGraphDataProperty, value);
        }
        public static readonly DependencyProperty WriteGraphDataProperty =
            DependencyProperty.Register(nameof(WriteGraphData), typeof(ObservableCollection<GraphPointViewModel>), typeof(GraphsWidget), new PropertyMetadata(null));


        public string ReadGraphSubheader
        {
            get => (string)GetValue(ReadGraphSubheaderProperty);
            set => SetValue(ReadGraphSubheaderProperty, value);
        }
        public static readonly DependencyProperty ReadGraphSubheaderProperty =
            DependencyProperty.Register(nameof(ReadGraphSubheader), typeof(string), typeof(GraphsWidget), new PropertyMetadata(null));


        public string WriteGraphSubheader
        {
            get => (string)GetValue(WriteGraphSubheaderProperty);
            set => SetValue(WriteGraphSubheaderProperty, value);
        }
        public static readonly DependencyProperty WriteGraphSubheaderProperty =
            DependencyProperty.Register(nameof(WriteGraphSubheader), typeof(string), typeof(GraphsWidget), new PropertyMetadata(null));
    }
}
