using System.Windows.Input;
using SecureFolderFS.Shared.Extensions;
#if ANDROID
using Microsoft.Maui.Controls.Shapes;
#endif

namespace SecureFolderFS.Maui.Views.Modals
{
    public partial class BaseModalPage : ContentPage
    {
        private Guid? _primaryToolbarItemId;
        
        public BaseModalPage()
        {
            BindingContext = this;
            InitializeComponent();
        }

        private static void UpdateButtonsVisibility(BindableObject bindable)
        {
#if ANDROID
            if (bindable is not BaseModalPage modalPage)
                return;
            
            var primaryButton = modalPage.PrimaryButton;
            var closeButton = modalPage.CloseButton;
            var buttonsGrid = modalPage.ButtonsGrid;

            var closeVisible = !string.IsNullOrEmpty(modalPage.CloseText);
            var primaryVisible = !string.IsNullOrEmpty(modalPage.PrimaryText);

            modalPage.ButtonsGrid.IsVisible = primaryVisible || closeVisible;
            if (!primaryVisible && !closeVisible)
                return;

            switch (primaryVisible, closeVisible)
            {
                case (true, true):
                {
                    buttonsGrid.SetColumn(primaryButton, 1);
                    buttonsGrid.SetColumnSpan(primaryButton, 1);
                    buttonsGrid.SetColumnSpan(closeButton, 1);
                    break;
                }

                case (true, false):
                {
                    buttonsGrid.SetColumn(primaryButton, 0);
                    buttonsGrid.SetColumnSpan(primaryButton, 2);
                    break;
                }

                case (false, true):
                {
                    buttonsGrid.SetColumnSpan(closeButton, 2);
                    break;
                }
            }
#else
            _ = bindable;
#endif
        }

        public bool IsImmersive
        {
            get => (bool)GetValue(IsImmersiveProperty);
            set => SetValue(IsImmersiveProperty, value);
        }
        public static readonly BindableProperty IsImmersiveProperty =
            BindableProperty.Create(nameof(IsImmersive), typeof(bool), typeof(BaseModalPage), false, propertyChanged:
                static (bindable, _, newValue) =>
                {
#if ANDROID
                    if (bindable is not BaseModalPage modalPage)
                        return;

                    modalPage.ModalBorder.StrokeShape = !(bool)newValue
                        ? new RoundRectangle() { CornerRadius = new(24, 24, 0, 0) }
                        : new Rectangle();
#endif
                });

        public View? ModalContent
        {
            get => (View?)GetValue(ModalContentProperty);
            set => SetValue(ModalContentProperty, value);
        }
        public static readonly BindableProperty ModalContentProperty =
            BindableProperty.Create(nameof(ModalContent), typeof(View), typeof(BaseModalPage));

        public ICommand? PrimaryCommand
        {
            get => (ICommand?)GetValue(PrimaryCommandProperty);
            set => SetValue(PrimaryCommandProperty, value);
        }
        public static readonly BindableProperty PrimaryCommandProperty =
            BindableProperty.Create(nameof(PrimaryCommand), typeof(ICommand), typeof(BaseModalPage));

        public ICommand? CloseCommand
        {
            get => (ICommand?)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }
        public static readonly BindableProperty CloseCommandProperty =
            BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(BaseModalPage));

        public string? PrimaryText
        {
            get => (string?)GetValue(PrimaryTextProperty);
            set => SetValue(PrimaryTextProperty, value);
        }
        public static readonly BindableProperty PrimaryTextProperty =
            BindableProperty.Create(nameof(PrimaryText), typeof(string), typeof(BaseModalPage),
                propertyChanged: static (bindable, oldValue, newValue) =>
                {
                    UpdateButtonsVisibility(bindable);
#if IOS
                    if (bindable is not BaseModalPage modalPage)
                        return;

                    if (newValue is null && oldValue is string)
                    {
                        var removed = modalPage.ToolbarItems.RemoveMatch(x => Equals(x.Id, modalPage._primaryToolbarItemId));
                        if (removed is not null)
                        {
                            removed.RemoveBinding(MenuItem.TextProperty);
                            removed.RemoveBinding(MenuItem.CommandProperty);
                            removed.RemoveBinding(MenuItem.IsEnabledProperty);
                        }
                    }
                    else if (oldValue is null && newValue is not null)
                    {
                        var toolbarItem = new ToolbarItem()
                        {
                            Order = ToolbarItemOrder.Primary
                        };
                        
                        toolbarItem.SetBinding(MenuItem.TextProperty, new Binding(nameof(PrimaryText), mode: BindingMode.OneWay, source: modalPage));
                        toolbarItem.SetBinding(MenuItem.CommandProperty, new Binding(nameof(PrimaryCommand), mode: BindingMode.OneWay, source: modalPage));
                        toolbarItem.SetBinding(MenuItem.IsEnabledProperty, new Binding(nameof(PrimaryEnabled), mode: BindingMode.OneWay, source: modalPage));

                        modalPage.ToolbarItems.Add(toolbarItem);
                        modalPage._primaryToolbarItemId = toolbarItem.Id;
                    }
#endif
                });

        public string? CloseText
        {
            get => (string?)GetValue(CloseTextProperty);
            set => SetValue(CloseTextProperty, value);
        }
        public static readonly BindableProperty CloseTextProperty =
            BindableProperty.Create(nameof(CloseText), typeof(string), typeof(BaseModalPage),
                propertyChanged: static (bindable, _, _) => UpdateButtonsVisibility(bindable));

        public bool PrimaryEnabled
        {
            get => (bool)GetValue(PrimaryEnabledProperty);
            set => SetValue(PrimaryEnabledProperty, value);
        }
        public static readonly BindableProperty PrimaryEnabledProperty =
            BindableProperty.Create(nameof(PrimaryEnabled), typeof(bool), typeof(BaseModalPage), true);

        public bool CloseEnabled
        {
            get => (bool)GetValue(CloseEnabledProperty);
            set => SetValue(CloseEnabledProperty, value);
        }
        public static readonly BindableProperty CloseEnabledProperty =
            BindableProperty.Create(nameof(CloseEnabled), typeof(bool), typeof(BaseModalPage), true);
    }
}
