using Avalonia.Markup.Xaml.Templates;
using SecureFolderFS.AvaloniaUI.UserControls.InfoBars;
using SecureFolderFS.Sdk.ViewModels.Controls;

namespace SecureFolderFS.AvaloniaUI.TemplateSelectors
{
    internal sealed class FileSystemInfoBarTemplateSelector : GenericTemplateSelector<InfoBarViewModel>
    {
        public DataTemplate? DokanyUnavailableInfoBarTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(InfoBarViewModel? item)
        {
            if (item is DokanyInfoBar)
                return DokanyUnavailableInfoBarTemplate;

            return null;
        }
    }
}