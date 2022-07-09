using System;
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
        public string Name { get; }

        /// <inheritdoc/>
        public CultureInfo Culture { get; }

        public AppLanguageModel(string languageTag)
        {
            LanguageTag = languageTag;
            Culture = new(languageTag);
            Name = FormatFriendlyName(Culture.NativeName);
        }

        private static string FormatFriendlyName(string unformatted)
        {
            return string.Concat(unformatted[0].ToString().ToUpperInvariant(), unformatted.AsSpan(1));
        }
    }
}
