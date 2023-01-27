using System.Collections;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SecureFolderFS.AvaloniaUI.UserControls.Widgets
{
    internal sealed partial class GraphsWidget : UserControl
    {
        private readonly SemaphoreSlim _graphClickSemaphore;

        public GraphsWidget()
        {
            InitializeComponent();
            _graphClickSemaphore = new(1, 1);
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
                HideColumn(GraphsGrid.ColumnDefinitions[2]);
            else
                RestoreColumn(GraphsGrid.ColumnDefinitions[2]);
        }

        private void WriteGraph_OnClick(object? sender, RoutedEventArgs e)
        {
            WriteGraphIsExtended = !WriteGraphIsExtended;
            if (WriteGraphIsExtended)
                HideColumn(GraphsGrid.ColumnDefinitions[0]);
            else
                RestoreColumn(GraphsGrid.ColumnDefinitions[0]);
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