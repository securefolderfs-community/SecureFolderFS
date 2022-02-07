using System.Text;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Models.Transitions;
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

        public IRelayCommand<string> UnlockVaultCommand { get; }

        public VaultLoginPageViewModel(VaultModel vaultModel)
            : base(new WeakReferenceMessenger(), vaultModel)
        {
            this._VaultName = vaultModel.VaultName;

            this.UnlockVaultCommand = new RelayCommand<string?>(UnlockVault);
        }

        private void UnlockVault(string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                // TODO: Please provide password
                WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(VaultModel, new VaultDashboardPageViewModel(Messenger, VaultModel)) { Transition = new DrillInTransitionModel() });
                return;
            }
            else
            {
                var disposablePassword = new DisposablePassword(Encoding.UTF8.GetBytes(password));
                // TODO: PasswordClearRequestedMessage

                IFinalizedVaultLoadRoutine finalizedVaultLoadRoutine;
                try
                {
                    var step5 = VaultRoutines.NewVaultLoadRoutine()
                        .EstablishRoutine()
                        .AddVaultPath(new(VaultModel.VaultRootPath))
                        .AddFileOperations()
                        .FindConfigurationFile()
                        .ContinueConfigurationFileInitialization();

                    IVaultLoadRoutineStep6 step6;
                    if (!File.Exists(Path.Combine(VaultModel.VaultRootPath!,
                            SecureFolderFS.Core.Constants.VAULT_KEYSTORE_FILENAME)))
                    {
                        // TODO: Ask for the keystore file
                        // DoubleFA dfa = new();
                        // dfa.AskForKeystore(); // ??
                        IVaultKeystoreDiscoverer keystoreDiscoverer = null;

                        step6 = step5.FindKeystoreFile(true, keystoreDiscoverer);
                    }
                    else
                    {
                        step6 = step5.FindKeystoreFile();
                    }

                    finalizedVaultLoadRoutine = step6.ContinueKeystoreFileInitialization()
                        .AddEncryptionAlgorithmBuilder()
                        .DeriveMasterKeyFromPassword(disposablePassword)
                        .ContinueInitializationWithMasterKey()
                        .VerifyVaultConfiguration()
                        .ContinueInitialization()
                        .Finish();
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

                var vaultDashboardPageViewModel = new VaultDashboardPageViewModel(Messenger, VaultModel);
                vaultDashboardPageViewModel.InitializeWithRoutine(finalizedVaultLoadRoutine);

                WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(VaultModel, vaultDashboardPageViewModel) { Transition = new DrillInTransitionModel() });
            }
        }
    }
}
