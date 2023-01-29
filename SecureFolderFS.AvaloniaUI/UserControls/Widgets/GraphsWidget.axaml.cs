using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using SecureFolderFS.AvaloniaUI.Animations;

namespace SecureFolderFS.AvaloniaUI.UserControls.Widgets
{
    internal sealed partial class GraphsWidget : UserControl
    {
        public GraphsWidget()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void HideColumn(ColumnDefinition column)
        {
            GraphsGrid.ColumnDefinitions[1].Width = new(0, GridUnitType.Star);
            column.Width = new(0, GridUnitType.Star);
        }

        private void RestoreColumn(ColumnDefinition column)
        {
            GraphsGrid.ColumnDefinitions[1].Width = new(8, GridUnitType.Pixel);
            column.Width = new(1, GridUnitType.Star);
        }

        private void ReadGraph_OnClick(object? sender, RoutedEventArgs e)
        {
            ReadGraphIsExtended = !ReadGraphIsExtended;
            if (ReadGraphIsExtended)
                _ = ExtendGraphAsync(ReadGraph, WriteGraph, GraphsGrid.ColumnDefinitions[2]);
            else
                _ = RetractGraphAsync(ReadGraph, WriteGraph, GraphsGrid.ColumnDefinitions[2]);
        }

        private void WriteGraph_OnClick(object? sender, RoutedEventArgs e)
        {
            WriteGraphIsExtended = !WriteGraphIsExtended;
            if (WriteGraphIsExtended)
                _ = ExtendGraphAsync(WriteGraph, ReadGraph, GraphsGrid.ColumnDefinitions[0]);
            else
                _ = RetractGraphAsync(WriteGraph, ReadGraph, GraphsGrid.ColumnDefinitions[0]);
        }

        private async Task ExtendGraphAsync(GraphControl clickedGraph, GraphControl otherGraph, ColumnDefinition column)
        {
            _ = HideGraphAsync(otherGraph);
            await Task.Delay(90);
            HideColumn(column);
            await ExtendGraphAsync(clickedGraph);
        }

        private async Task RetractGraphAsync(GraphControl clickedGraph, GraphControl otherGraph, ColumnDefinition column)
        {
            RestoreColumn(column);
            _ = RetractGraphAsync(clickedGraph);
            await Task.Delay(30);
            await RestoreGraphAsync(otherGraph);
        }

        private Task HideGraphAsync(GraphControl graph)
        {
            HideGraphStoryboard.Animations[0].Target = graph;
            HideGraphStoryboard.Animations[1].Target = graph;

            return HideGraphStoryboard.RunAnimationsAsync();
        }

        private Task ExtendGraphAsync(GraphControl graph)
        {
            ExtendGraphStoryboard.Animations[0].Target = graph;
            return ExtendGraphStoryboard.RunAnimationsAsync();
        }

        private Task RetractGraphAsync(GraphControl graph)
        {
            RetractGraphStoryboard.Animations[0].Target = graph;
            return RetractGraphStoryboard.RunAnimationsAsync();
        }

        private Task RestoreGraphAsync(GraphControl graph)
        {
            RestoreGraphStoryboard.Animations[0].Target = graph;
            RestoreGraphStoryboard.Animations[1].Target = graph;

            return RestoreGraphStoryboard.RunAnimationsAsync();
        }

        public bool ReadGraphIsExtended
        {
            get => GetValue(ReadGraphIsExtendedProperty);
            set => SetValue(ReadGraphIsExtendedProperty, value);
        }
        public static readonly StyledProperty<bool> ReadGraphIsExtendedProperty =
            AvaloniaProperty.Register<GraphsWidget, bool>(nameof(ReadGraphIsExtended));

        public bool WriteGraphIsExtended
        {
            get => GetValue(WriteGraphIsExtendedProperty);
            set => SetValue(WriteGraphIsExtendedProperty, value);
        }
        public static readonly StyledProperty<bool> WriteGraphIsExtendedProperty =
            AvaloniaProperty.Register<GraphsWidget, bool>(nameof(WriteGraphIsExtended));

        public string? ReadGraphSubheader
        {
            get => GetValue(ReadGraphSubheaderProperty);
            set => SetValue(ReadGraphSubheaderProperty, value);
        }
        public static readonly StyledProperty<string?> ReadGraphSubheaderProperty =
            AvaloniaProperty.Register<GraphsWidget, string?>(nameof(ReadGraphSubheader));

        public string? WriteGraphSubheader
        {
            get => GetValue(WriteGraphSubheaderProperty);
            set => SetValue(WriteGraphSubheaderProperty, value);
        }
        public static readonly StyledProperty<string?> WriteGraphSubheaderProperty =
            AvaloniaProperty.Register<GraphsWidget, string?>(nameof(WriteGraphSubheader));

        public IList? ReadGraphData
        {
            get => GetValue(ReadGraphDataProperty);
            set => SetValue(ReadGraphDataProperty, value);
        }
        public static readonly StyledProperty<IList?> ReadGraphDataProperty =
            AvaloniaProperty.Register<GraphsWidget, IList?>(nameof(ReadGraphData));

        public IList? WriteGraphData
        {
            get => GetValue(WriteGraphDataProperty);
            set => SetValue(WriteGraphDataProperty, value);
        }
        public static readonly StyledProperty<IList?> WriteGraphDataProperty =
            AvaloniaProperty.Register<GraphsWidget, IList?>(nameof(WriteGraphData));
    }
}