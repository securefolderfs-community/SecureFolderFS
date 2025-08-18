using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Accounts.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.Accounts.ViewModels
{
    [Bindable(true)]
    public abstract partial class AccountViewModel : ObservableObject, IViewable, IRemoteResource<IFolder>, ISavePersistence
    {
        protected readonly AccountDataModel? accountDataModel;
        protected readonly IPropertyStore<string>? propertyStore;
        private IFolder? _folder;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsConnected;
        [ObservableProperty] private bool _IsInputFilled;
        [ObservableProperty] private bool _IsProgressing;

        /// <summary>
        /// Gets the unique identifier for the account associated with this view model.
        /// </summary>
        public string AccountId { get; }

        /// <summary>
        /// Gets the type (unique identifier) of the data source associated with the account.
        /// </summary>
        public abstract string DataSourceType { get; }

        protected AccountViewModel(AccountDataModel accountDataModel, IPropertyStore<string> propertyStore)
            : this(accountDataModel.AccountId ?? throw new NullReferenceException(nameof(AccountDataModel.AccountId)), propertyStore, accountDataModel.DisplayName)
        {
            this.accountDataModel = accountDataModel;
        }

        protected AccountViewModel(string accountId, IPropertyStore<string> propertyStore, string? title)
        {
            AccountId = accountId;
            Title = title;
            this.propertyStore = propertyStore;
        }

        /// <inheritdoc/>
        public async Task<IFolder> ConnectAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IsProgressing = true;
                if (_folder is not null && await IsConnectedAsync(cancellationToken))
                    return _folder;

                if (accountDataModel is null || propertyStore is null)
                    return await ConnectFromUserInputAsync(cancellationToken);

                return await ConnectFromDataModelAsync(cancellationToken);
            }
            finally
            {
                IsProgressing = false;
            }
        }

        /// <inheritdoc/>
        public virtual async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(propertyStore);

            // Get existing IDs
            var value = await propertyStore.GetValueAsync<string?>(DataSourceType, null, cancellationToken);
            var accountIds = await StreamSerializer.Instance.TryDeserializeFromStringAsync<string[]>(value ?? string.Empty, cancellationToken) ?? [];

            // Modify existing
            var newIds = accountIds.Append(AccountId).ToArray();

            // Serialize and set
            var serialized = await StreamSerializer.Instance.SerializeToStringAsync(newIds, cancellationToken);
            await propertyStore.SetValueAsync(DataSourceType, serialized, cancellationToken);

            // Save account properties
            await SaveAccountAsync(cancellationToken);

            // Flush, if supported
            if (propertyStore is ISavePersistence savePersistence)
                await savePersistence.SaveAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously checks if the account is currently connected and accessible.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task result contains a boolean value indicating whether the account is connected.</returns>
        public virtual async Task<bool> IsConnectedAsync(CancellationToken cancellationToken)
        {
            try
            {
                IsProgressing = true;
                return IsConnected = await CheckConnectionAsync(cancellationToken);
            }
            catch
            {
                return false;
            }
            finally
            {
                IsProgressing = false;
            }
        }

        protected abstract Task<IFolder> ConnectFromUserInputAsync(CancellationToken cancellationToken);

        protected abstract Task<IFolder> ConnectFromDataModelAsync(CancellationToken cancellationToken);

        protected abstract Task DisconnectAsync(CancellationToken cancellationToken);

        protected abstract Task<bool> CheckConnectionAsync(CancellationToken cancellationToken);

        protected abstract Task SaveAccountAsync(CancellationToken cancellationToken);

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            _folder = null;
            DisconnectAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public virtual async ValueTask DisposeAsync()
        {
            _folder = null;
            await DisconnectAsync(CancellationToken.None);
        }
    }
}