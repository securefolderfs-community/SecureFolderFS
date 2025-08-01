using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Results;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard
{
    [Inject<IVaultCredentialsService>, Inject<IVaultManagerService>, Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class CredentialsWizardViewModel : OverlayViewModel, IStagingView
    {
        private readonly string _vaultId;
        private readonly TaskCompletionSource<IKey> _credentialsTcs;

        [ObservableProperty] private bool _IsNameCipherEnabled;
        [ObservableProperty] private PickerOptionViewModel? _ContentCipher;
        [ObservableProperty] private PickerOptionViewModel? _FileNameCipher;
        [ObservableProperty] private PickerOptionViewModel? _EncodingOption;
        [ObservableProperty] private ObservableCollection<PickerOptionViewModel> _ContentCiphers = new();
        [ObservableProperty] private ObservableCollection<PickerOptionViewModel> _FileNameCiphers = new();
        [ObservableProperty] private ObservableCollection<PickerOptionViewModel> _EncodingOptions = new();
        [ObservableProperty] private ObservableCollection<AuthenticationViewModel> _AuthenticationOptions = new();
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;

        public IModifiableFolder Folder { get; }

        public CredentialsWizardViewModel(IModifiableFolder folder)
        {
            ServiceProvider = DI.Default;
            Folder = folder;
            _credentialsTcs = new();
            _RegisterViewModel = new(AuthenticationStage.FirstStageOnly);
            _vaultId = Guid.NewGuid().ToString();

            Title = "SetCredentials".ToLocalized();
            PrimaryText = "Continue".ToLocalized();
            SecondaryText = "Cancel".ToLocalized();
            CanContinue = false;
            CanCancel = true;

            RegisterViewModel.PropertyChanged += RegisterViewModel_PropertyChanged;
            RegisterViewModel.CredentialsProvided += RegisterViewModel_CredentialsProvided;
        }

        /// <inheritdoc/>
        public async Task<IResult> TryContinueAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ContentCipher);
            ArgumentNullException.ThrowIfNull(FileNameCipher);
            ArgumentNullException.ThrowIfNull(EncodingOption);
            ArgumentNullException.ThrowIfNull(RegisterViewModel.CurrentViewModel);

            // Await the credentials
            RegisterViewModel.ConfirmCredentialsCommand.Execute(null);
            var credentials = await _credentialsTcs.Task;

            // Make sure to also dispose the data within the current view model whether the navigation is successful or not
            using (RegisterViewModel.CurrentViewModel)
            {
                // We don't need to set the Version property since the creator will always initialize with the latest one
                var vaultOptions = new VaultOptions()
                {
                    UnlockProcedure = new AuthenticationMethod([ RegisterViewModel.CurrentViewModel.Id ], null),
                    ContentCipherId = ContentCipher.Id,
                    FileNameCipherId = FileNameCipher.Id,
                    NameEncodingId = EncodingOption.Id,
                    RecycleBinSize = 0L,
                    VaultId = _vaultId
                };

                // Create the vault
                var unlockContract = await VaultManagerService.CreateAsync(
                    Folder,
                    credentials,
                    vaultOptions,
                    cancellationToken);

                return new CredentialsResult(unlockContract, _vaultId);
            }
        }

        /// <inheritdoc/>
        public Task<IResult> TryCancelAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IResult>(Result.Success);
        }

        /// <inheritdoc/>
        public override async void OnAppearing()
        {
            // Get options
            EnumerateOptions(VaultCredentialsService.GetContentCiphers(), ContentCiphers);
            EnumerateOptions(VaultCredentialsService.GetFileNameCiphers(), FileNameCiphers);
            EnumerateOptions(VaultCredentialsService.GetEncodingOptions(), EncodingOptions);

            // Set default cipher options
            ContentCipher = ContentCiphers.FirstOrDefault();
            FileNameCipher = FileNameCiphers.FirstOrDefault();
            EncodingOption = EncodingOptions.FirstOrDefault();

            // Get authentication options
            AuthenticationOptions.Clear();
            await foreach (var item in VaultCredentialsService.GetCreationAsync(Folder, _vaultId))
                AuthenticationOptions.Add(item);

            // Set default authentication option
            RegisterViewModel.CurrentViewModel = AuthenticationOptions.FirstOrDefault();
            return;

            static void EnumerateOptions(IEnumerable<string> source, ICollection<PickerOptionViewModel> destination)
            {
                destination.Clear();
                foreach (var item in source)
                {
                    var name = string.IsNullOrEmpty(item) ? "NoEncryption".ToLocalized() : item;
                    destination.Add(new(item, name));
                }
            }
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
        {
            RegisterViewModel.PropertyChanged -= RegisterViewModel_PropertyChanged;
            RegisterViewModel.CredentialsProvided -= RegisterViewModel_CredentialsProvided;
            AuthenticationOptions.DisposeElements();
            RegisterViewModel.Dispose();
        }

        partial void OnFileNameCipherChanged(PickerOptionViewModel? value)
        {
            IsNameCipherEnabled = !string.IsNullOrEmpty(value?.Id);
        }

        private void RegisterViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            _credentialsTcs.TrySetResult(e.Authentication);
        }

        private void RegisterViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RegisterViewModel.CanContinue))
                CanContinue = RegisterViewModel.CanContinue;
        }
    }
}
