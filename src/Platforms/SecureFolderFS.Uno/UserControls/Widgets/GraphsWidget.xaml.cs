using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SecureFolderFS.Sdk.ViewModels.Controls.Widgets.Categories;
using SecureFolderFS.Uno.Extensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.Widgets
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
            var result = !ReadGraphIsExtended
                ? await GraphExtendAnimationAsync(ReadGraph, WriteGraph, WriteColumn)
                : await GraphRetractAnimationAsync(ReadGraph, WriteGraph, WriteColumn);

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

        private void RestoreGraphsState()
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

        private void GraphsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreGraphsState();
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
            DependencyProperty.Register(nameof(ReadGraphIsExtended), typeof(bool), typeof(GraphsWidget), new PropertyMetadata(defaultValue: false));

        public bool WriteGraphIsExtended
        {
            get => (bool)GetValue(WriteGraphIsExtendedProperty);
            set => SetValue(WriteGraphIsExtendedProperty, value);
        }
        public static readonly DependencyProperty WriteGraphIsExtendedProperty =
            DependencyProperty.Register(nameof(WriteGraphIsExtended), typeof(bool), typeof(GraphsWidget), new PropertyMetadata(defaultValue: false));

        public IList<GraphPoint>? ReadGraphData
        {
            get => (IList<GraphPoint>?)GetValue(ReadGraphDataProperty);
            set => SetValue(ReadGraphDataProperty, value);
        }
        public static readonly DependencyProperty ReadGraphDataProperty =
            DependencyProperty.Register(nameof(ReadGraphData), typeof(IList<GraphPoint>), typeof(GraphsWidget), new PropertyMetadata(defaultValue: null));

        public IList<GraphPoint>? WriteGraphData
        {
            get => (IList<GraphPoint>?)GetValue(WriteGraphDataProperty);
            set => SetValue(WriteGraphDataProperty, value);
        }
        public static readonly DependencyProperty WriteGraphDataProperty =
            DependencyProperty.Register(nameof(WriteGraphData), typeof(IList<GraphPoint>), typeof(GraphsWidget), new PropertyMetadata(defaultValue: null));

        public string? ReadGraphSubheader
        {
            get => (string?)GetValue(ReadGraphSubheaderProperty);
            set => SetValue(ReadGraphSubheaderProperty, value);
        }
        public static readonly DependencyProperty ReadGraphSubheaderProperty =
            DependencyProperty.Register(nameof(ReadGraphSubheader), typeof(string), typeof(GraphsWidget), new PropertyMetadata(defaultValue: null));

        public string? WriteGraphSubheader
        {
            get => (string?)GetValue(WriteGraphSubheaderProperty);
            set => SetValue(WriteGraphSubheaderProperty, value);
        }
        public static readonly DependencyProperty WriteGraphSubheaderProperty =
            DependencyProperty.Register(nameof(WriteGraphSubheader), typeof(string), typeof(GraphsWidget), new PropertyMetadata(defaultValue: null));
    }
}
