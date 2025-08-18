using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Services
{
    public interface IAccountService
    {
        IAsyncEnumerable<AccountViewModel> GetAccountsAsync(string dataSourceIdentifier, IPropertyStore<string> propertyStore, CancellationToken cancellationToken = default);

        Task<AccountViewModel> GetAccountCreatorAsync(string dataSourceIdentifier, IPropertyStore<string> propertyStore, CancellationToken cancellationToken = default);
    }
}
