namespace SecureFolderFS.Maui.UserControls
{
    public partial class ContentPresentation : ContentView, IDisposable
    {
        public ContentPresentation()
        {
            InitializeComponent();
        }
        
        /// <inheritdoc/>
        public void Dispose()
        {
            (MainContent.Content as IDisposable)?.Dispose();
            if (MainContent.Content is not null)
                MainContent.Content.BindingContext = null;
            
            MainContent.Content = null;
        }

        public object? Presentation
        {
            get => (object?)GetValue(PresentationProperty);
            set => SetValue(PresentationProperty, value);
        }
        public static readonly BindableProperty PresentationProperty =
            BindableProperty.Create(nameof(Presentation), typeof(object), typeof(ContentPresentation), propertyChanged:
                static (bindable, _, newValue) => ApplyTemplate(bindable, newValue));

        public DataTemplateSelector? TemplateSelector
        {
            get => (DataTemplateSelector?)GetValue(TemplateSelectorProperty);
            set => SetValue(TemplateSelectorProperty, value);
        }
        public static readonly BindableProperty TemplateSelectorProperty =
            BindableProperty.Create(nameof(TemplateSelector), typeof(DataTemplateSelector), typeof(ContentPresentation), propertyChanged:
                static (bindable, _, _) => ApplyTemplate(bindable, (bindable as ContentPresentation)?.Presentation));

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
