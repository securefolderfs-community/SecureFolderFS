using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed class LanguageViewModel : ObservableObject
    {
        private readonly ILanguageModel _languageModel;

        public string FriendlyName => _languageModel.Name;

        public LanguageViewModel(ILanguageModel languageModel)
        {
            _languageModel = languageModel;
        }
    }
}
