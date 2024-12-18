using SecureFolderFS.Sdk.ViewModels.Controls.Previewers;

namespace SecureFolderFS.Maui.TemplateSelectors
{
    internal sealed class PreviewerTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ImageTemplate { get; set; }
        
        public DataTemplate? VideoTemplate { get; set; }
        
        public DataTemplate? TextTemplate { get; set; }
        
        /// <inheritdoc/>
        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            return item switch
            {
                ImagePreviewerViewModel => ImageTemplate,
                VideoPreviewerViewModel => VideoTemplate,
                TextPreviewerViewModel => TextTemplate,
                _ => null
            };
        }
    }
}
