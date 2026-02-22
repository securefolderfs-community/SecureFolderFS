using System.Globalization;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Tests.ServiceImplementation
{
    /// <inheritdoc cref="ILocalizationService"/>
    internal sealed class MockLocalizationService : ILocalizationService
    {
        private static readonly Dictionary<string, string> Resources = new()
        {
            { "DateToday", "Today, {0}" },
            { "DateYesterday", "Yesterday, {0}" },
            { "DateDaysAgo", "{0} days ago" },
            { "DateWeekAgo", "Last week" }
        };

        /// <inheritdoc/>
        public CultureInfo CurrentCulture { get; } = CultureInfo.InvariantCulture;

        /// <inheritdoc/>
        public IReadOnlyList<CultureInfo> AppLanguages { get; } = [CultureInfo.InvariantCulture];

        /// <inheritdoc/>
        public string? GetResource(string resourceKey)
        {
            return Resources.GetValueOrDefault(resourceKey);
        }

        /// <inheritdoc/>
        public Task SetCultureAsync(CultureInfo cultureInfo)
        {
            return Task.CompletedTask;
        }
    }
}

