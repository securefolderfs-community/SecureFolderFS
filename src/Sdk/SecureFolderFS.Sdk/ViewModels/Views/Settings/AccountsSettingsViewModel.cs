using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Bindable(true)]
    public sealed class AccountsSettingsViewModel : BaseSettingsViewModel
    {
        /// <summary>
        /// Gets the accounts managed on this device, aggregated across all registered providers.
        /// </summary>
        public ObservableCollection<AccountItemViewModel> Accounts { get; } = new();

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Account providers are optional and platform-dependent; resolve the collection defensively.
            var providers = DI.OptionalService<IEnumerable<IAccountProvider>>();

            // Gather everything before touching the collection,
            // so concurrent InitAsync calls can't interleave into duplicates.
            var items = new List<AccountItemViewModel>();
            if (providers is not null)
            {
                foreach (var provider in providers)
                {
                    foreach (var account in await provider.GetAccountsAsync(cancellationToken))
                        items.Add(new AccountItemViewModel(account, provider, Accounts));
                }
            }

            Accounts.Clear();
            foreach (var item in items)
                Accounts.Add(item);
        }
    }
}
