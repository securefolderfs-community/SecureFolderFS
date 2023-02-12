using System;
using System.Collections.Generic;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    // TODO implement
    /// <inheritdoc cref="ILocalizationService"/>
    internal sealed class LocalizationService : ILocalizationService
    {
        /// <inheritdoc/>
        public ILanguageModel CurrentLanguage { get; } = new AppLanguageModel("en-US");

        /// <inheritdoc/>
        public string? GetString(string resourceKey)
        {
            return resourceKey;
        }

        /// <inheritdoc/>
        public void SetCurrentLanguage(ILanguageModel language)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<ILanguageModel> GetLanguages()
        {
            return new[] { CurrentLanguage };
        }
    }
}