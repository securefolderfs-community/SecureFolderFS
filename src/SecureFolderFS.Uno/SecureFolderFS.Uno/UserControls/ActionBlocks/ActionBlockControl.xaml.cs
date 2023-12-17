using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecureFolderFS.Uno.Enums;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.Uno.UserControls.ActionBlocks
{
    public sealed partial class ActionBlockControl : UserControl
    {
        public ActionBlockControl()
        {
            InitializeComponent();
        }

        private void Expander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            ExpanderExpandingCommand?.Execute(null);
        }

        public ActionBlockMode BlockMode
        {
            get => (ActionBlockMode)GetValue(BlockModeProperty);
            set => SetValue(BlockModeProperty, value);
        }
        public static readonly DependencyProperty BlockModeProperty =
            DependencyProperty.Register(nameof(BlockMode), typeof(ActionBlockMode), typeof(ActionBlockControl), new PropertyMetadata(ActionBlockMode.Default));

        public ICommand? ClickCommand
        {
            get => (ICommand?)GetValue(ClickCommandProperty);
            set => SetValue(ClickCommandProperty, value);
        }
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(ActionBlockControl), new PropertyMetadata(defaultValue: null));

        public ICommand? ExpanderExpandingCommand
        {
            get => (ICommand?)GetValue(ExpanderExpandingCommandProperty);
            set => SetValue(ExpanderExpandingCommandProperty, value);
        }
        public static readonly DependencyProperty ExpanderExpandingCommandProperty =
            DependencyProperty.Register(nameof(ExpanderExpandingCommand), typeof(ICommand), typeof(ActionBlockControl), new PropertyMetadata(defaultValue: null));

        public FrameworkElement ExpanderContent
        {
            get => (FrameworkElement)GetValue(ExpanderContentProperty);
            set => SetValue(ExpanderContentProperty, value);
        }
        public static readonly DependencyProperty ExpanderContentProperty =
            DependencyProperty.Register(nameof(ExpanderContent), typeof(FrameworkElement), typeof(ActionBlockControl), new PropertyMetadata(defaultValue: null));

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(ActionBlockControl), new PropertyMetadata(defaultValue: false));

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(IconElement), typeof(ActionBlockControl), new PropertyMetadata(defaultValue: null));

        public string? Title
        {
            get => (string?)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ActionBlockControl), new PropertyMetadata(defaultValue: null));

        public string? Description
        {
            get => (string?)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(ActionBlockControl), new PropertyMetadata(defaultValue: null));

        public FrameworkElement CustomDescription
        {
            get => (FrameworkElement)GetValue(CustomDescriptionProperty);
            set => SetValue(CustomDescriptionProperty, value);
        }
        public static readonly DependencyProperty CustomDescriptionProperty =
            DependencyProperty.Register(nameof(CustomDescription), typeof(FrameworkElement), typeof(ActionBlockControl), new PropertyMetadata(defaultValue: null));

        public FrameworkElement ActionElement
        {
            get => (FrameworkElement)GetValue(ActionElementProperty);
            set => SetValue(ActionElementProperty, value);
        }
        public static readonly DependencyProperty ActionElementProperty =
            DependencyProperty.Register(nameof(ActionElement), typeof(FrameworkElement), typeof(ActionBlockControl), new PropertyMetadata(defaultValue: null));

        public FrameworkElement AdditionalActionElement
        {
            get => (FrameworkElement)GetValue(AdditionalActionElementProperty);
            set => SetValue(AdditionalActionElementProperty, value);
        }
        public static readonly DependencyProperty AdditionalActionElementProperty =
            DependencyProperty.Register(nameof(AdditionalActionElement), typeof(FrameworkElement), typeof(ActionBlockControl), new PropertyMetadata(defaultValue: null));
    }
}
