using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Accounts.ViewModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.DataSources
{
    [Bindable(true)]
    public sealed partial class AccountCreationWizardViewModel : OverlayViewModel, IStagingView
    {
        [ObservableProperty] private AccountViewModel _AccountViewModel;

        public AccountCreationWizardViewModel(AccountViewModel accountViewModel)
        {
            AccountViewModel = accountViewModel;
            CanContinue = AccountViewModel.IsInputFilled;
            Title = "AddAccount".ToLocalized();
            PrimaryText = "Continue".ToLocalized();
        }

        /// <inheritdoc/>
        public async Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            if (!AccountViewModel.IsInputFilled)
                return Result.Failure(new ArgumentException($"'{nameof(AccountViewModel.IsInputFilled)}' is false."));

            try
            {
                var folder = await AccountViewModel.ConnectAsync(cancellationToken);
                await AccountViewModel.SaveAsync(cancellationToken);

                return Result<IFolder>.Success(folder);
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            await AccountViewModel.DisposeAsync();
            return Result.Success;
        }

        /// <inheritdoc/>
        public override void OnAppearing()
        {
            AccountViewModel.PropertyChanged += AccountViewModel_PropertyChanged;
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
        {
            AccountViewModel.PropertyChanged -= AccountViewModel_PropertyChanged;
        }

        private void AccountViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AccountViewModel.IsInputFilled))
                CanContinue = AccountViewModel.IsInputFilled;
        }
    }
}
