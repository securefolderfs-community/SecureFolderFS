using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.UI.UserControls.InfoBars;

namespace SecureFolderFS.Uno.TemplateSelectors
{
    internal sealed class FileSystemInfoBarTemplateSelector : BaseTemplateSelector<InfoBarViewModel>
    {
        public DataTemplate? DokanyUnavailableInfoBarTemplate { get; set; }

        public DataTemplate? WebDavExperimentalInfoBarTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(InfoBarViewModel? item, DependencyObject container)
        {
            return item switch
            {
                DokanyInfoBar => DokanyUnavailableInfoBarTemplate,
                WebDavInfoBar => WebDavExperimentalInfoBarTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
