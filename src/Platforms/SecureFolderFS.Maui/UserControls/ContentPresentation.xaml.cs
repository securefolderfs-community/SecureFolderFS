namespace SecureFolderFS.Maui.UserControls
{
    public partial class ContentPresentation : ContentView
    {
        public ContentPresentation()
        {
            InitializeComponent();
        }

        public object? ViewContent
        {
            get => (object?)GetValue(ViewContentProperty);
            set => SetValue(ViewContentProperty, value);
        }
        public static readonly BindableProperty ViewContentProperty =
            BindableProperty.Create(nameof(ViewContent), typeof(object), typeof(ContentPresentation), null, propertyChanged:
                (bindable, _, newValue) => ApplyTemplate(bindable, newValue));

        public DataTemplateSelector? TemplateSelector
        {
            get => (DataTemplateSelector?)GetValue(TemplateSelectorProperty);
            set => SetValue(TemplateSelectorProperty, value);
        }
        public static readonly BindableProperty TemplateSelectorProperty =
            BindableProperty.Create(nameof(TemplateSelector), typeof(DataTemplateSelector), typeof(ContentPresentation), null, propertyChanged:
                (bindable, _, _) => ApplyTemplate(bindable, (bindable as ContentPresentation)?.ViewContent));

        private static void ApplyTemplate(BindableObject bindable, object? newValue)
        {
            if (bindable is not ContentPresentation presentation)
                return;

            var template = presentation.TemplateSelector?.SelectTemplate(newValue, bindable);
            if (template is null)
                return;

            presentation.MainContent.Content = template.CreateContent() as View;
            if (presentation.MainContent.Content is not null)
                presentation.MainContent.Content.BindingContext = newValue;
        }
    }
}
