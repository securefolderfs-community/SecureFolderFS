using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Helpers;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources
{
    [Inject<IAccountService>, Inject<IPropertyStoreService>]
    [Bindable(true)]
    public partial class AccountSourceWizardViewModel : BaseDataSourceWizardViewModel, INavigatable, INavigator
    {
        private IFolder? _selectedFolder;

        [ObservableProperty] private string? _Message;
        [ObservableProperty] private string? _SelectedLocation;
        [ObservableProperty] private AccountViewModel? _SelectedAccount;
        [ObservableProperty] private ObservableCollection<AccountViewModel> _Accounts;

        /// <inheritdoc/>
        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public AccountSourceWizardViewModel(string dataSourceType, string dataSourceName, NewVaultMode mode, IVaultCollectionModel vaultCollectionModel)
            : base(dataSourceType, mode, vaultCollectionModel)
        {
            ServiceProvider = DI.Default;
            Accounts = new();
            DataSourceName = dataSourceName;
            PrimaryText = "Continue".ToLocalized();
            Title = "ChooseAccount".ToLocalized();
        }

        /// <inheritdoc/>
        public override async void OnAppearing()
        {
            if (!Accounts.IsEmpty())
                return;

            var accounts = await AccountService.GetAccountsAsync(DataSourceType, PropertyStoreService.SecurePropertyStore).ToArrayAsyncImpl();
            Accounts.DisposeAll();
            Accounts.Clear();
            Accounts.AddMultiple(accounts);
        }

        /// <inheritdoc/>
        public override Task<IFolder?> GetFolderAsync()
        {
            return Task.FromResult(_selectedFolder);
        }

        /// <inheritdoc/>
        public override Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public override VaultStorageSourceDataModel? ToStorageSource()
        {
            if (SelectedAccount is null)
                return null;

            return new AccountSourceDataModel(SelectedAccount.AccountId, DataSourceType);
        }

        /// <inheritdoc/>
        public async Task<bool> NavigateAsync(IViewDesignation? view)
        {
            var tcs = new TaskCompletionSource<bool>();
            NavigationRequested?.Invoke(this, new DestinationNavigationRequestedEventArgs(this, view, tcs));

            return await tcs.Task;
        }

        /// <inheritdoc/>
        public Task<bool> GoBackAsync()
        {
            NavigationRequested?.Invoke(this, new BackNavigationRequestedEventArgs(this));
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> GoForwardAsync()
        {
            return Task.FromResult(false);
        }

        [RelayCommand]
        private async Task AddAccountAsync(CancellationToken cancellationToken)
        {
            var accountViewModel = await AccountService.GetAccountCreatorAsync(DataSourceType, PropertyStoreService.SecurePropertyStore, cancellationToken);
            NavigationRequested?.Invoke(this, new DestinationNavigationRequestedEventArgs(this, new AccountCreationWizardViewModel(accountViewModel)));
        }

        [RelayCommand]
        private async Task RemoveAccountAsync(AccountViewModel? accountViewModel, CancellationToken cancellationToken)
        {
            if (accountViewModel is null || DataSourceName is null)
                return;

            // Get the property store
            var propertyStore = PropertyStoreService.SecurePropertyStore;

            // Remove account data
            await propertyStore.RemoveAsync(accountViewModel.AccountId, cancellationToken);

            // Get the raw account list
            var rawAccountIds = await propertyStore.GetValueAsync<string?>(DataSourceName, null, cancellationToken);
            if (rawAccountIds is null)
                return;

            // Update the account list
            var accountIds = await StreamSerializer.Instance.TryDeserializeFromStringAsync<string[]?>(rawAccountIds, cancellationToken);
            var newAccountIds = accountIds?.Where(x => x != accountViewModel.AccountId).ToArray() ?? [];

            // Serialize and store the new account list
            var serialized = await StreamSerializer.Instance.SerializeToStringAsync(newAccountIds, cancellationToken);
            await propertyStore.SetValueAsync(DataSourceName, serialized, cancellationToken);

            // Remove from the list
            await accountViewModel.DisposeAsync();
            Accounts.Remove(accountViewModel);
        }

        [RelayCommand]
        private async Task SelectAccountAsync(AccountViewModel? accountViewModel, CancellationToken cancellationToken)
        {
            if (accountViewModel is null)
                return;

            try
            {
                var isConnected = await accountViewModel.IsConnectedAsync(cancellationToken);
                if (!isConnected)
                {
                    // Disconnect other accounts
                    SelectedAccount = null;
                    foreach (var account in Accounts)
                        await account.DisposeAsync();

                    // Try to connect to the account
                    await accountViewModel.ConnectAsync(cancellationToken);
                    SelectedAccount = accountViewModel;
                    return;
                }

                // It is expected that the existing connection will return the root folder immediately
                var rootFolder = await accountViewModel.ConnectAsync(cancellationToken);
                var browser = BrowserHelpers.CreateBrowser(rootFolder, new FileSystemOptions(), accountViewModel, outerNavigator: this);

                // Prompt the user to pick a folder
                _selectedFolder = await browser.PickFolderAsync(null, true, cancellationToken);
                if (_selectedFolder is null)
                    return;

                // Update CanContinue
                var result = await ValidationHelpers.ValidateAddedVault(_selectedFolder, Mode, VaultCollectionModel.Select(x => x.DataModel), cancellationToken);
                Message = result.Message;
                CanContinue = result.CanContinue;
                SelectedLocation = result.SelectedLocation;
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            NavigationRequested = null;
            Accounts.Where(x => x != SelectedAccount).DisposeAll();
            Accounts.Clear();
        }
    }
}