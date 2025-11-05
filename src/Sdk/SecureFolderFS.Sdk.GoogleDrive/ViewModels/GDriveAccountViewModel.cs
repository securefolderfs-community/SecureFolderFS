using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Accounts.DataModels;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Sdk.GoogleDrive.Storage;
using SecureFolderFS.Shared.Api;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.GoogleDrive.ViewModels
{
    [Bindable(true)]
    public sealed partial class GDriveAccountViewModel : AccountViewModel
    {
        private DriveService? _driveService;

        /// <inheritdoc/>
        public override string DataSourceType { get; } = Constants.DATA_SOURCE_GOOGLE_DRIVE;

        public GDriveAccountViewModel(AccountDataModel accountDataModel, IPropertyStore<string> propertyStore)
            : base(accountDataModel, propertyStore)
        {
        }

        public GDriveAccountViewModel(string accountId, IPropertyStore<string> propertyStore, string? title)
            : base(accountId, propertyStore, title)
        {
        }

        /// <inheritdoc/>
        protected override Task<IFolder> ConnectFromUserInputAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        protected override async Task<IFolder> ConnectFromDataModelAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();
            if (_driveService is not null)
                return new GDriveFolder(_driveService, "/", string.Empty);

            ArgumentNullException.ThrowIfNull(propertyStore);
            ArgumentNullException.ThrowIfNull(accountDataModel);

            if (string.IsNullOrEmpty(accountDataModel.AccountId))
                throw new ArgumentException("AccountId cannot be empty.");

            var rawData = await propertyStore.GetValueAsync<string?>(accountDataModel.AccountId, null, cancellationToken);
            if (string.IsNullOrEmpty(rawData))
                throw new ArgumentException("Data cannot be empty.");


        }

        /// <inheritdoc/>
        protected override Task DisconnectAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Task<bool> CheckConnectionAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Task SaveAccountAsync(CancellationToken cancellationToken)
        {
            AssertApiAccess();
            throw new System.NotImplementedException();
        }

        public static void AssertApiAccess()
        {
            if (string.IsNullOrEmpty(ApiKeys.GoogleDriveClientKey))
                throw new InvalidOperationException("Google Drive API key is not set.");
        }
    }
}
