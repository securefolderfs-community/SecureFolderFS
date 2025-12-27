using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentFTP;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Accounts.DataModels;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Sdk.Ftp.DataModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.Ftp.ViewModels
{
    [Bindable(true)]
    public sealed partial class FtpAccountViewModel : AccountViewModel
    {
        private AsyncFtpClient? _ftpClient;

        [ObservableProperty] private string? _Address;
        [ObservableProperty] private string? _UserName;
        [ObservableProperty] private string? _Password;

        /// <inheritdoc/>
        public override string DataSourceType { get; } = Constants.DATA_SOURCE_FTP;

        public FtpAccountViewModel(AccountDataModel dataModel, IPropertyStore<string> propertyStore)
            : base(dataModel, propertyStore)
        {
        }

        public FtpAccountViewModel(string id, IPropertyStore<string> propertyStore, string? title)
            : base(id, propertyStore, title)
        {
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromUserInputAsync(CancellationToken cancellationToken)
        {
            if (_ftpClient is not null)
                return new FtpFolder(_ftpClient, "/", string.Empty);

            return await ConnectAsync(Address, UserName, Password, cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromDataModelAsync(CancellationToken cancellationToken)
        {
            if (_ftpClient is not null)
                return new FtpFolder(_ftpClient, "/", string.Empty);

            ArgumentNullException.ThrowIfNull(propertyStore);
            ArgumentNullException.ThrowIfNull(accountDataModel);

            if (string.IsNullOrEmpty(accountDataModel.AccountId))
                throw new ArgumentException("AccountId cannot be empty.");

            var rawData = await propertyStore.GetValueAsync<string?>(accountDataModel.AccountId, null, cancellationToken);
            if (string.IsNullOrEmpty(rawData))
                throw new ArgumentException("Data cannot be empty.");

            var ftpAccountDataModel = await StreamSerializer.Instance.DeserializeFromStringAsync<FtpAccountDataModel?>(rawData, cancellationToken);
            if (ftpAccountDataModel is null)
                throw new ArgumentException("Data cannot be deserialized.");

            // Update properties from the data model (except password for security reasons)
            Address = ftpAccountDataModel.Address;
            UserName = ftpAccountDataModel.UserName;

            // Connect using the data model
            return await ConnectAsync(ftpAccountDataModel.Address, ftpAccountDataModel.UserName, ftpAccountDataModel.Password, cancellationToken);
        }

        /// <inheritdoc/>
        protected override async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (_ftpClient is null)
                return;

            await _ftpClient.DisposeAsync();
            _ftpClient = null;
            IsConnected = false;
        }

        /// <inheritdoc/>
        protected override async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken)
        {
            if (_ftpClient is null)
                return false;

            var isStillConnected = await _ftpClient.IsStillConnected(3000, cancellationToken);
            if (!isStillConnected)
                await DisconnectAsync(cancellationToken);

            return isStillConnected;
        }

        /// <inheritdoc/>
        protected override async Task SaveAccountAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(propertyStore);
            var dataModel = new FtpAccountDataModel(AccountId, DataSourceType, UserName)
            {
                Address = Address,
                UserName = UserName,
                Password = Password
            };

            // Serialize and set
            var serialized = await StreamSerializer.Instance.SerializeToStringAsync(dataModel, cancellationToken);
            await propertyStore.SetValueAsync(AccountId, serialized, cancellationToken);
        }

        private async Task<IFolder> ConnectAsync(string? address, string? username, string? password, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Address and username cannot be empty.");

            try
            {
                var uri = new Uri(address);
                var config = new FtpConfig();

                // Sandbox may block automatic detection of PASV, so it's necessary to be set manually
                if (OperatingSystem.IsIOS())
                    config.DataConnectionType = FtpDataConnectionType.PASV;

                _ftpClient = new AsyncFtpClient(uri.Host, username, password ?? string.Empty, uri.Port, config);
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(3000));
                await _ftpClient.Connect(timeoutCts.Token);
                IsConnected = true;

                return new FtpFolder(_ftpClient, "/", string.Empty);
            }
            catch (Exception)
            {
                await DisconnectAsync(cancellationToken);
                throw;
            }
        }

        private void UpdateInputValidation()
        {
            IsInputFilled = !string.IsNullOrEmpty(Address) && !string.IsNullOrEmpty(UserName);
        }

        partial void OnAddressChanged(string? value)
        {
            _ = value;
            UpdateInputValidation();
        }

        partial void OnUserNameChanged(string? value)
        {
            _ = value;
            UpdateInputValidation();
        }
    }
}