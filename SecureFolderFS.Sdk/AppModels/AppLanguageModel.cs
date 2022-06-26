using SecureFolderFS.Shared.Extensions;
using System.Globalization;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="ILanguageModel"/>
    public sealed class AppLanguageModel : ILanguageModel
    {
        /// <inheritdoc/>
        public string LanguageTag { get; }

        /// <inheritdoc/>
        public string FriendlyName { get; }

        /// <inheritdoc/>
        public CultureInfo Culture { get; }

        public AppLanguageModel(string languageTag)
        {
            LanguageTag = languageTag;
            Culture = new(languageTag);
            FriendlyName = Culture.NativeName.FirstToUpper();
        }
    }
}
