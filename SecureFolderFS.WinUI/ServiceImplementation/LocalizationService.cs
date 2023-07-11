using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Windows.Globalization;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
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
            CurrentCulture = new(SupportedLanguages.First(x => x.Contains(ApplicationLanguages.PrimaryLanguageOverride)));
            AppLanguages = GetAppLanguages().ToImmutableList();
            ResourceManager = new($"SecureFolderFS.UI.Strings.{GetLanguageString(CurrentCulture)}.Resources", typeof(UI.Constants).Assembly);
        }

        /// <inheritdoc/>
        public override Task SetCultureAsync(CultureInfo cultureInfo)
        {
            ApplicationLanguages.PrimaryLanguageOverride = cultureInfo.Name;
            return Task.CompletedTask;
        }

        private IEnumerable<CultureInfo> GetAppLanguages()
        {
            foreach (var item in ApplicationLanguages.ManifestLanguages)
            {
                yield return new(SupportedLanguages.First(x => x.Contains(item)));
            }
        }
    }
}
