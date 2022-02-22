using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.VaultLoader.Discoverers.KeystoreDiscovery;
using SecureFolderFS.Core.VaultLoader.Routine;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages
{
    public sealed class VaultLoginPageViewModel : BasePageViewModel
    {
        private string? _VaultName;
        public string? VaultName
        {
            get => _VaultName;
            set => SetProperty(ref _VaultName, value);
        }

        public IRelayCommand UnlockVaultCommand { get; }

        public VaultLoginPageViewModel(VaultViewModel vaultModel)
            : base(new WeakReferenceMessenger(), vaultModel)
        {
            this._VaultName = vaultModel.VaultName;

            this.UnlockVaultCommand = new RelayCommand<DisposablePassword?>(UnlockVault);
        }

        private void UnlockVault(DisposablePassword? password)
        {
            if (password == null || password.Length == 0)
            {
                password?.Dispose();
                VaultViewModel.VaultModel.LastOpened = DateTime.Now;
                WeakReferenceMessenger.Default.Send(new VaultSerializationRequestedMessage(VaultViewModel));
                WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(VaultViewModel, new VaultDashboardPageViewModel(Messenger, VaultViewModel)));
                return;
            }

            // TODO: PasswordClearRequestedMessage

            IFinalizedVaultLoadRoutine finalizedVaultLoadRoutine;
            try
            {
                var step5 = VaultRoutines.NewVaultLoadRoutine()
                    .EstablishRoutine()
                    .AddVaultPath(new(VaultViewModel.VaultRootPath))
                    .AddFileOperations()
                    .FindConfigurationFile()
                    .ContinueConfigurationFileInitialization();

                IVaultLoadRoutineStep6 step6;
                if (!File.Exists(Path.Combine(VaultViewModel.VaultRootPath!, SecureFolderFS.Core.Constants.VAULT_KEYSTORE_FILENAME)))
                {
                    // TODO: Ask for the keystore file
                    // DoubleFA dfa = new();
                    // if (dfa.IsEnabledForVault(VaultModel)) dfa.AskForKeystore(); // ??
                    IVaultKeystoreDiscoverer keystoreDiscoverer = null;

                    step6 = step5.FindKeystoreFile(true, keystoreDiscoverer);
                }
                else
                {
                    step6 = step5.FindKeystoreFile();
                }

                finalizedVaultLoadRoutine = step6.ContinueKeystoreFileInitialization()
                    .AddEncryptionAlgorithmBuilder()
                    .DeriveMasterKeyFromPassword(password)
                    .ContinueInitializationWithMasterKey()
                    .VerifyVaultConfiguration()
                    .ContinueInitialization()
                    .Finalize();
            }
            catch (FileNotFoundException)
            {
                // TODO: Vault is corrupted (configuration file not found), show message
                return;
            }
            catch (UnsupportedVaultException)
            {
                // TODO: Vault version is unsupported by SecureFolderFS
                return;
            }
            catch (IncorrectPasswordException)
            {
                // TODO: The password is incorrect, show info
                return;
            }
            catch (UnauthenticVaultConfigurationException)
            {
                // TODO: The vault has been tampered, show message
                return;
            }
            finally
            {
                password.Dispose();
            }

            var vaultDashboardPageViewModel = new VaultDashboardPageViewModel(Messenger, VaultViewModel);

            VaultViewModel.VaultModel.LastOpened = DateTime.Now;
            WeakReferenceMessenger.Default.Send(new VaultSerializationRequestedMessage(VaultViewModel));
            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(VaultViewModel, vaultDashboardPageViewModel));

            vaultDashboardPageViewModel.InitializeWithRoutine(finalizedVaultLoadRoutine);

        }
    }
}
