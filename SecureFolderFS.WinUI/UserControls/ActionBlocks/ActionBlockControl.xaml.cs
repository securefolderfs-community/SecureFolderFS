using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SecureFolderFS.WinUI.UserControls.ActionBlocks
{
    public sealed partial class ActionBlockControl : UserControl
    {
        public ActionBlockControl()
        {
            this.InitializeComponent();
        }

        public IRelayCommand ButtonCommand
        {
            get => (IRelayCommand)GetValue(ButtonCommandProperty);
            set => SetValue(ButtonCommandProperty, value);
        }
        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register("ButtonCommand", typeof(IRelayCommand), typeof(ActionBlockControl), new PropertyMetadata(null));


        public IRelayCommand ExpanderExpandingCommand
        {
            get => (IRelayCommand)GetValue(ExpanderExpandingCommandProperty);
            set => SetValue(ExpanderExpandingCommandProperty, value);
        }
        public static readonly DependencyProperty ExpanderExpandingCommandProperty =
            DependencyProperty.Register("ExpanderExpandingCommand", typeof(IRelayCommand), typeof(ActionBlockControl), new PropertyMetadata(null));


        public FrameworkElement ExpanderContent
        {
            get => (FrameworkElement)GetValue(ExpanderContentProperty);
            set => SetValue(ExpanderContentProperty, value);
        }
        public static readonly DependencyProperty ExpanderContentProperty =
            DependencyProperty.Register("ExpanderContent", typeof(FrameworkElement), typeof(ActionBlockControl), new PropertyMetadata(null));


        public bool IsClickable
        {
            get => (bool)GetValue(IsClickableProperty);
            set => SetValue(IsClickableProperty, value);
        }
        public static readonly DependencyProperty IsClickableProperty =
            DependencyProperty.Register("IsClickable", typeof(bool), typeof(ActionBlockControl), new PropertyMetadata(false));


        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(IconElement), typeof(ActionBlockControl), new PropertyMetadata(null));


        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(ActionBlockControl), new PropertyMetadata(null));


        public FrameworkElement AdditionalDescription
        {
            get => (FrameworkElement)GetValue(AdditionalDescriptionProperty);
            set => SetValue(AdditionalDescriptionProperty, value);
        }
        public static readonly DependencyProperty AdditionalDescriptionProperty =
            DependencyProperty.Register("AdditionalDescription", typeof(FrameworkElement), typeof(ActionBlockControl), new PropertyMetadata(null));


        public FrameworkElement ActionElement
        {
            get => (FrameworkElement)GetValue(ActionElementProperty);
            set => SetValue(ActionElementProperty, value);
        }
        public static readonly DependencyProperty ActionElementProperty =
            DependencyProperty.Register("ActionElement", typeof(FrameworkElement), typeof(ActionBlockControl), new PropertyMetadata(null));


        public FrameworkElement AdditionalActionElement
        {
            get => (FrameworkElement)GetValue(AdditionalActionElementProperty);
            set => SetValue(AdditionalActionElementProperty, value);
        }
        public static readonly DependencyProperty AdditionalActionElementProperty =
            DependencyProperty.Register("AdditionalActionElement", typeof(FrameworkElement), typeof(ActionBlockControl), new PropertyMetadata(null));

        private void Expander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            ExpanderExpandingCommand?.Execute(null);
        }
    }
}
