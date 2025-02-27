using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls
{
    public partial class SplitCell : ViewCell
    {
        private readonly Label _leftLabel;
        private readonly Label _rightLabel;

        public SplitCell()
        {
            InitializeComponent();

            _leftLabel = new()
            {
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16d
            };
            _rightLabel = new()
            {
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16d,
                Opacity = 0.6d
            };
            RootGrid.BindingContext = this;
        }

        private void SplitCell_Tapped(object? sender, EventArgs e)
        {
            Command?.Execute(CommandParameter);
        }

        private static void SetText(BindableObject bindable, bool isRightSide, object? newValue)
        {
            if (bindable is not SplitCell splitCell)
                throw new InvalidOperationException($"Bindable is not of type {nameof(SplitCell)}.");

            var text = newValue as string;
            if (isRightSide)
            {
                splitCell._rightLabel.Text = text ?? string.Empty;
                splitCell.RightSide = text is null ? null : splitCell._rightLabel;
            }
            else
            {
                splitCell._leftLabel.Text = text ?? string.Empty;
                splitCell.LeftSide = text is null ? null : splitCell._leftLabel;
            }
        }

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SplitCell), null);

        public object? CommandParameter
        {
            get => (object?)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SplitCell), null);

        public View? LeftSide
        {
            get => (View?)GetValue(LeftSideProperty);
            set => SetValue(LeftSideProperty, value);
        }
        public static readonly BindableProperty LeftSideProperty =
            BindableProperty.Create(nameof(LeftSide), typeof(View), typeof(SplitCell), null);

        public View? RightSide
        {
            get => (View?)GetValue(RightSideProperty);
            set => SetValue(RightSideProperty, value);
        }
        public static readonly BindableProperty RightSideProperty =
            BindableProperty.Create(nameof(RightSide), typeof(View), typeof(SplitCell), null);

        public string? LeftText
        {
            get => (string?)GetValue(LeftTextProperty);
            set => SetValue(LeftTextProperty, value);
        }
        public static readonly BindableProperty LeftTextProperty =
            BindableProperty.Create(nameof(LeftText), typeof(string), typeof(SplitCell), null, propertyChanged:
                (bindable, _, newValue) => SetText(bindable, false, newValue));

        public string? RightText
        {
            get => (string?)GetValue(RightTextProperty);
            set => SetValue(RightTextProperty, value);
        }
        public static readonly BindableProperty RightTextProperty =
            BindableProperty.Create(nameof(RightText), typeof(string), typeof(SplitCell), null, propertyChanged:
                (bindable, _, newValue) => SetText(bindable, true, newValue));
    }
}
