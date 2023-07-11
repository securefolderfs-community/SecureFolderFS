using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    // TODO: Implement localization
    /// <inheritdoc cref="ILocalizationService"/>
    internal sealed class LocalizationService : BaseLocalizationService
    {
        /// <inheritdoc/>
        protected override ResourceManager ResourceManager { get; }

        /// <inheritdoc/>
        public override CultureInfo CurrentCulture { get; }

        /// <inheritdoc/>
        public override IReadOnlyList<CultureInfo> AppLanguages { get; }

        public LocalizationService()
        {
            CurrentCulture = new("en-US");
            AppLanguages = new List<CultureInfo>() { CurrentCulture };
        }

        /// <inheritdoc/>
        public override Task SetCultureAsync(CultureInfo cultureInfo)
        {
            return Task.CompletedTask;
        }
    }
}