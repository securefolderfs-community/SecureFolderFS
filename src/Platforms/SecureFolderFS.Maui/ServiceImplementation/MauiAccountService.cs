using System.Runtime.CompilerServices;
using SecureFolderFS.Sdk.Accounts.DataModels;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Ftp.ViewModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using static SecureFolderFS.Sdk.Ftp.Constants;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IAccountService"/>
    internal sealed class MauiAccountService : IAccountService
    {
        /// <inheritdoc/>
        public async IAsyncEnumerable<AccountViewModel> GetAccountsAsync(string dataSourceIdentifier, IPropertyStore<string> propertyStore, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var value = await propertyStore.GetValueAsync<string?>(dataSourceIdentifier, null, cancellationToken);
            if (value is null)
                yield break;
            
            var accountIds = await StreamSerializer.Instance.TryDeserializeFromStringAsync<string[]>(value, cancellationToken);
            if (accountIds.IsEmpty())
                yield break;

            // Process each account ID
            foreach (var accountId in accountIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
             
                var rawAccountData = await propertyStore.GetValueAsync<string?>(accountId, null, cancellationToken);
                if (rawAccountData is null)
                    continue;
                
                var accountData = await StreamSerializer.Instance.TryDeserializeFromStringAsync<AccountDataModel?>(rawAccountData, cancellationToken);
                if (accountData is null)
                    continue;

                yield return accountData.DataSourceType switch
                {
                    DATA_SOURCE_FTP => new FtpAccountViewModel(accountData, propertyStore),
                    
                    // TODO: Maybe move AccountViewModel to shared project and implement the AccountViewModel
                    // in separate projects (SecureFolderFS.Sdk.Ftp, SecureFolderFS.Sdk.GoogleDrive)
                    _ => throw new ArgumentOutOfRangeException(nameof(AccountDataModel.DataSourceType))
                };
            }
        }

        /// <inheritdoc/>
        public async Task<AccountViewModel> GetAccountCreatorAsync(string dataSourceIdentifier, IPropertyStore<string> propertyStore, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return dataSourceIdentifier switch
            {
                DATA_SOURCE_FTP => new FtpAccountViewModel(Guid.NewGuid().ToString(), propertyStore, "FTP".ToLocalized()),
                _ => throw new ArgumentOutOfRangeException(nameof(dataSourceIdentifier))
            };
        }
    }
}
