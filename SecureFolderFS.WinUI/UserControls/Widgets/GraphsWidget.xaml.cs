using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        private async Task StartGraphHideStoryboard(UIElement element)
        {
            Storyboard.SetTarget(GraphHideStoryboard.Children[0], element);
            Storyboard.SetTarget(GraphHideStoryboard.Children[1], element);
            await GraphHideStoryboard.BeginAsync();
            element.Visibility = Visibility.Collapsed;
            GraphHideStoryboard.Stop();
        }

        private async Task StartGraphExtendStoryboard(UIElement element)
        {
            Storyboard.SetTarget(GraphExtendStoryboard.Children[0], element);
            await GraphExtendStoryboard.BeginAsync();
            GraphExtendStoryboard.Stop();
        }

        private async Task StartGraphRetractStoryboard(UIElement element)
        {
            Storyboard.SetTarget(GraphRetractStoryboard.Children[0], element);
            await GraphRetractStoryboard.BeginAsync();
            GraphRetractStoryboard.Stop();
        }

        private async Task StartGraphRestoreStoryboard(UIElement element)
        {
            Storyboard.SetTarget(GraphRestoreStoryboard.Children[0], element);
            Storyboard.SetTarget(GraphRestoreStoryboard.Children[1], element);
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
            bool result;
            if (!ReadGraphIsExtended)
                result = await GraphExtendAnimationAsync(ReadGraph, WriteGraph, WriteColumn);
            else
                result = await GraphRetractAnimationAsync(ReadGraph, WriteGraph, WriteColumn);

            ReadGraphIsExtended = result ? !ReadGraphIsExtended : ReadGraphIsExtended;
        }

        private async void WriteGraph_Click(object sender, RoutedEventArgs e)
        {
            bool result;
            if (!WriteGraphIsExtended)
                result = await GraphExtendAnimationAsync(WriteGraph, ReadGraph, ReadColumn);
            else
                result = await GraphRetractAnimationAsync(WriteGraph, ReadGraph, ReadColumn);

            WriteGraphIsExtended = result ? !WriteGraphIsExtended : WriteGraphIsExtended;
        }

        private async Task<bool> GraphExtendAnimationAsync(UIElement clickedGraph, UIElement otherGraph, ColumnDefinition column)
        {
            if (!await _graphClickSemaphore.WaitAsync(0))
                return false;

            try
            {
                _ = StartGraphHideStoryboard(otherGraph);
                await Task.Delay(90);
                GraphsGrid.ColumnSpacing = 0;
                HideColumn(column);
                await StartGraphExtendStoryboard(clickedGraph);

                return true;
            }
            finally
            {
                _graphClickSemaphore.Release();
            }
        }

        private async Task<bool> GraphRetractAnimationAsync(UIElement clickedGraph, UIElement otherGraph, ColumnDefinition column)
        {
            if (!await _graphClickSemaphore.WaitAsync(0))
                return false;

            try
            {
                RestoreColumn(column);
                _ = StartGraphRetractStoryboard(clickedGraph);
                await Task.Delay(30);
                await StartGraphRestoreStoryboard(otherGraph);

                return true;
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

        /// <inheritdoc/>
        public void Dispose()
        {
            _graphClickSemaphore.Dispose();
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

        public IList<GraphPointViewModel>? ReadGraphData
        {
            get => (IList<GraphPointViewModel>?)GetValue(ReadGraphDataProperty);
            set => SetValue(ReadGraphDataProperty, value);
        }
        public static readonly DependencyProperty ReadGraphDataProperty =
            DependencyProperty.Register(nameof(ReadGraphData), typeof(IList<GraphPointViewModel>), typeof(GraphsWidget), new PropertyMetadata(null));

        public IList<GraphPointViewModel>? WriteGraphData
        {
            get => (IList<GraphPointViewModel>?)GetValue(WriteGraphDataProperty);
            set => SetValue(WriteGraphDataProperty, value);
        }
        public static readonly DependencyProperty WriteGraphDataProperty =
            DependencyProperty.Register(nameof(WriteGraphData), typeof(IList<GraphPointViewModel>), typeof(GraphsWidget), new PropertyMetadata(null));

        public string? ReadGraphSubheader
        {
            get => (string?)GetValue(ReadGraphSubheaderProperty);
            set => SetValue(ReadGraphSubheaderProperty, value);
        }
        public static readonly DependencyProperty ReadGraphSubheaderProperty =
            DependencyProperty.Register(nameof(ReadGraphSubheader), typeof(string), typeof(GraphsWidget), new PropertyMetadata(null));

        public string? WriteGraphSubheader
        {
            get => (string?)GetValue(WriteGraphSubheaderProperty);
            set => SetValue(WriteGraphSubheaderProperty, value);
        }
        public static readonly DependencyProperty WriteGraphSubheaderProperty =
            DependencyProperty.Register(nameof(WriteGraphSubheader), typeof(string), typeof(GraphsWidget), new PropertyMetadata(null));
    }
}
