using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.PasswordRequest;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.VaultLoader.Discoverers.KeystoreDiscovery;
using SecureFolderFS.Core.VaultLoader.Routine;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault
{
    public sealed class VaultLoginPageViewModel : BaseVaultPageViewModel
    {
        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        public IAsyncRelayCommand UnlockVaultCommand { get; }

        public VaultLoginPageViewModel(IVaultModel vaultModel)
            : base(new WeakReferenceMessenger(), vaultModel)
        {
            UnlockVaultCommand = new AsyncRelayCommand<DisposablePassword?>(UnlockVaultAsync);
        }

        private async Task UnlockVaultAsync(DisposablePassword? password)
        {
            if (password is null || password.Length == 0)
                return;

            IFinalizedVaultLoadRoutine finalizedVaultLoadRoutine;
            try
            {
                var step5 = VaultRoutines.NewVaultLoadRoutine()
                    .EstablishRoutine()
                    .AddVaultPath(new(VaultModel.Folder.Path))
                    .AddFileOperations()
                    .FindConfigurationFile()
                    .ContinueConfigurationFileInitialization();

                IVaultLoadRoutineStep6 step6;
                if (!await FileSystemService.FileExistsAsync(Path.Combine(VaultModel.Folder.Path, SecureFolderFS.Core.Constants.VAULT_KEYSTORE_FILENAME)))
                {
                    // TODO: Ask for the keystore file
                    // DoubleFA dfa = new();
                    // if (dfa.IsEnabledForVault(VaultModel)) dfa.AskForKeystore(); // ??
                    IVaultKeystoreDiscoverer? keystoreDiscoverer = null;

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

            // Ensure vault instance
            var vaultInstance = finalizedVaultLoadRoutine.ContinueWithOptionalRoutine()
                .EstablishOptionalRoutine()
                .AddFileSystemStatsTracker(null /* TODO: Add stats tracker */)
                .Finalize()
                .Deploy();

            var vaultViewModel = new VaultViewModel(VaultModel, vaultInstance, null);
            var vaultDashboard = new VaultDashboardPageViewModel(vaultViewModel, Messenger, VaultModel);

            _ = vaultDashboard.InitAsync();
            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(vaultDashboard));
        }
    }
}
