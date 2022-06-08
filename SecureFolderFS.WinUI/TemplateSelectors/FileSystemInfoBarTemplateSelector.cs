using Microsoft.UI.Xaml;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.FileSystemInfoBars;

namespace SecureFolderFS.WinUI.TemplateSelectors
{
    internal sealed class FileSystemInfoBarTemplateSelector : BaseTemplateSelector<InfoBarViewModel>
    {
        public DataTemplate? DokanyNotAvailableInfoBarTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(InfoBarViewModel? item, DependencyObject container)
        {
            if (item is DokanyInfoBarViewModel)
            {
                return DokanyNotAvailableInfoBarTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
