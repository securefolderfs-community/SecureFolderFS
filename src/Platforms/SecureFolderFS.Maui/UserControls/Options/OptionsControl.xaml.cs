using System.Windows.Input;

namespace SecureFolderFS.Maui.UserControls.Options
{
    public partial class OptionsControl : ContentView
    {
        private Label? _slotLabel;

        public OptionsControl()
        {
            InitializeComponent();
        }

        private static void SetSlotText(BindableObject bindable, object? newValue)
        {
            if (bindable is not OptionsControl optionsControl)
                throw new InvalidOperationException($"Bindable is not of type {nameof(OptionsControl)}.");

            var text = newValue as string;
            optionsControl._slotLabel ??= new()
            {
                VerticalOptions = LayoutOptions.Center,
                FontSize = 15d
            };

            optionsControl._slotLabel.Text = text ?? string.Empty;
            optionsControl.Slot = text is null ? null : optionsControl._slotLabel;
        }

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(OptionsControl));

        public object? Slot
        {
            get => (object?)GetValue(SlotProperty);
            set => SetValue(SlotProperty, value);
        }
        public static readonly BindableProperty SlotProperty =
            BindableProperty.Create(nameof(Slot), typeof(object), typeof(OptionsControl));

        public string? SlotText
        {
            get => (string?)GetValue(SlotTextProperty);
            set => SetValue(SlotTextProperty, value);
        }
        public static readonly BindableProperty SlotTextProperty =
            BindableProperty.Create(nameof(SlotText), typeof(string), typeof(OptionsControl), propertyChanged:
                static (bindable, _, newValue) => SetSlotText(bindable, newValue));

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(OptionsControl));

        public object? CommandParameter
        {
            get => (object?)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(OptionsControl));

        public bool IsSeparatorVisible
        {
            get => (bool)GetValue(IsSeparatorVisibleProperty);
            set => SetValue(IsSeparatorVisibleProperty, value);
        }
        public static readonly BindableProperty IsSeparatorVisibleProperty =
            BindableProperty.Create(nameof(IsSeparatorVisible), typeof(bool), typeof(OptionsControl), true);
    }
}

