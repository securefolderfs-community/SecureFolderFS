using FluentAssertions;
using NUnit.Framework;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.UI.ViewModels.Authentication;
using static SecureFolderFS.Core.Constants.Vault.Authentication;

namespace SecureFolderFS.Tests.VaultTests
{
    [TestFixture]
    public class CredentialTests
    {
        [Test]
        public async Task CreateVault_Password_UnlocksSuccessfully()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var passwordProcedure = new AuthenticationMethod([AUTH_PASSWORD], null);
            var options = CreateOptions(passwordProcedure, vaultId);
            using var initialPassword = await GetPasswordCreationCredentialAsync("Password#1");

            // Act
            using var _ = await manager.CreateAsync(vaultFolder, initialPassword, options);

            // Assert
            using var validPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            var canUnlock = await CanUnlockAsync(manager, vaultFolder, validPasskey);
            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);

            canUnlock.Should().BeTrue();
            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(passwordProcedure);
        }

        [Test]
        public async Task CreateVault_PasswordAndKeyFile_RequiresBothCredentials()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            using var compositePasskey = await GetCreationCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);

            var compositeProcedure = new AuthenticationMethod([AUTH_PASSWORD, AUTH_KEYFILE], null);
            var options = CreateOptions(compositeProcedure, vaultId);

            // Act
            using var _ = await manager.CreateAsync(vaultFolder, compositePasskey, options);

            // Assert
            using var correctPasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var missingKeyFilePasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var wrongPasswordPasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "WrongPassword", vaultId);

            (await CanUnlockAsync(manager, vaultFolder, correctPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, missingKeyFilePasskey)).Should().BeFalse();
            (await CanUnlockAsync(manager, vaultFolder, wrongPasswordPasskey)).Should().BeFalse();

            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(compositeProcedure);
        }

        [Test]
        public async Task ModifyAuthentication_PasswordChange_UsesNewPasswordAndRejectsOldPassword()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var passwordProcedure = new AuthenticationMethod([AUTH_PASSWORD], null);
            var options = CreateOptions(passwordProcedure, vaultId);
            using var originalPassword = await GetPasswordCreationCredentialAsync("Password#1");

            using var _ = await manager.CreateAsync(vaultFolder, originalPassword, options);

            using var unlockPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var unlockContract = await manager.UnlockAsync(vaultFolder, unlockPasskey);
            using var newPassword = await GetPasswordCreationCredentialAsync("Password#2");

            // Act
            await manager.ModifyAuthenticationAsync(vaultFolder, unlockContract, newPassword, options);

            // Assert
            using var oldPassword = await GetPasswordLoginCredentialAsync("Password#1");
            using var updatedPassword = await GetPasswordLoginCredentialAsync("Password#2");

            (await CanUnlockAsync(manager, vaultFolder, oldPassword)).Should().BeFalse();
            (await CanUnlockAsync(manager, vaultFolder, updatedPassword)).Should().BeTrue();
        }

        [Test]
        public async Task ModifyAuthentication_AddKeyFile_RequiresPasswordAndKeyFile()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var passwordOnlyProcedure = new AuthenticationMethod([AUTH_PASSWORD], null);
            var passwordAndKeyFileProcedure = new AuthenticationMethod([AUTH_PASSWORD, AUTH_KEYFILE], null);

            using var password = await GetPasswordCreationCredentialAsync("Password#1");
            using var _ = await manager.CreateAsync(vaultFolder, password, CreateOptions(passwordOnlyProcedure, vaultId));

            using var unlockPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var unlockContract = await manager.UnlockAsync(vaultFolder, unlockPasskey);
            using var newCompositePasskey = await GetCreationCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);

            // Act
            await manager.ModifyAuthenticationAsync(vaultFolder, unlockContract, newCompositePasskey, CreateOptions(passwordAndKeyFileProcedure, vaultId));

            // Assert
            using var fullCredentialPasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var passwordOnlyPasskey = await GetPasswordLoginCredentialAsync("Password#1");

            (await CanUnlockAsync(manager, vaultFolder, fullCredentialPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, passwordOnlyPasskey)).Should().BeFalse();

            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(passwordAndKeyFileProcedure);
        }

        [Test]
        public async Task ModifyAuthentication_RemoveKeyFile_RequiresPasswordOnly()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var compositeProcedure = new AuthenticationMethod([AUTH_PASSWORD, AUTH_KEYFILE], null);
            var passwordOnlyProcedure = new AuthenticationMethod([AUTH_PASSWORD], null);

            using var initialCompositePasskey = await GetCreationCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var _ = await manager.CreateAsync(vaultFolder, initialCompositePasskey, CreateOptions(compositeProcedure, vaultId));

            using var unlockPasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var unlockContract = await manager.UnlockAsync(vaultFolder, unlockPasskey);
            using var newPassword = await GetPasswordCreationCredentialAsync("Password#1");

            // Act
            await manager.ModifyAuthenticationAsync(vaultFolder, unlockContract, newPassword, CreateOptions(passwordOnlyProcedure, vaultId));

            // Assert
            using var passwordOnlyPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var oldCompositePasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);

            (await CanUnlockAsync(manager, vaultFolder, passwordOnlyPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, oldCompositePasskey)).Should().BeFalse();

            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(passwordOnlyProcedure);
        }

        [Test]
        public async Task CredentialsConfirmation_ChangeChainedPassword_PreservesKeyFile()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var compositeProcedure = new AuthenticationMethod([AUTH_PASSWORD, AUTH_KEYFILE], null);
            using var initialCompositePasskey = await GetCreationCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var _ = await manager.CreateAsync(vaultFolder, initialCompositePasskey, CreateOptions(compositeProcedure, vaultId));

            using var unlockPasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var unlockContract = await manager.UnlockAsync(vaultFolder, unlockPasskey);
            using var registerViewModel = new RegisterViewModel(AuthenticationStage.FirstStageOnly);
            using var confirmationViewModel = new CredentialsConfirmationViewModel(vaultFolder, registerViewModel, AuthenticationStage.FirstStageOnly)
            {
                UnlockContract = unlockContract,
                OldPasskey = unlockPasskey,
                OldAuthenticationMethodIds = [AUTH_PASSWORD, AUTH_KEYFILE]
            };

            registerViewModel.CurrentViewModel = CreatePasswordCreationViewModel("Password#2");

            // Act
            await confirmationViewModel.ConfirmAsync(CancellationToken.None);

            // Assert
            using var oldCompositePasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var updatedCompositePasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#2", vaultId);
            using var updatedPasswordOnlyPasskey = await GetPasswordLoginCredentialAsync("Password#2");

            (await CanUnlockAsync(manager, vaultFolder, oldCompositePasskey)).Should().BeFalse();
            (await CanUnlockAsync(manager, vaultFolder, updatedCompositePasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, updatedPasswordOnlyPasskey)).Should().BeFalse();

            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(compositeProcedure);
        }

        [Test]
        public async Task CredentialsConfirmation_ChangeChainedKeyFile_PreservesPassword()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var compositeProcedure = new AuthenticationMethod([AUTH_PASSWORD, AUTH_KEYFILE], null);
            using var initialCompositePasskey = await GetCreationCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var _ = await manager.CreateAsync(vaultFolder, initialCompositePasskey, CreateOptions(compositeProcedure, vaultId));

            using var unlockPasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var oldKeyFile = unlockPasskey.Keys.ElementAt(1).CreateCopy();
            using var unlockContract = await manager.UnlockAsync(vaultFolder, unlockPasskey);
            using var registerViewModel = new RegisterViewModel(AuthenticationStage.ProceedingStageOnly);
            using var confirmationViewModel = new CredentialsConfirmationViewModel(vaultFolder, registerViewModel, AuthenticationStage.ProceedingStageOnly)
            {
                UnlockContract = unlockContract,
                OldPasskey = unlockPasskey,
                OldAuthenticationMethodIds = [AUTH_PASSWORD, AUTH_KEYFILE]
            };

            var newKeyFile = await GetKeyFileCreationCredentialAsync(vaultId);
            registerViewModel.Credentials.Add(newKeyFile);
            registerViewModel.CurrentViewModel = new KeyFileCreationViewModel(vaultId);

            // Act
            await confirmationViewModel.ConfirmAsync(CancellationToken.None);

            // Assert
            using var updatedCompositePasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var oldCompositePasskey = new KeySequence();
            oldCompositePasskey.Add(await GetPasswordLoginCredentialAsync("Password#1"));
            oldCompositePasskey.Add(oldKeyFile.CreateCopy());

            (await CanUnlockAsync(manager, vaultFolder, updatedCompositePasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, oldCompositePasskey)).Should().BeFalse();

            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(compositeProcedure);
        }

        [Test]
        public async Task ModifyAuthentication_InvalidUnlockContract_ThrowsArgumentException()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var passwordProcedure = new AuthenticationMethod([AUTH_PASSWORD], null);
            using var originalPassword = await GetPasswordCreationCredentialAsync("Password#1");
            using var _ = await manager.CreateAsync(vaultFolder, originalPassword, CreateOptions(passwordProcedure, vaultId));

            var newPassword = await GetPasswordCreationCredentialAsync("Password#2");
            var invalidContract = await GetPasswordLoginCredentialAsync("not-a-contract");

            try
            {
                // Act
                Func<Task> action = () => manager.ModifyAuthenticationAsync(vaultFolder, invalidContract, newPassword, CreateOptions(passwordProcedure, vaultId));

                // Assert
                await action.Should().ThrowAsync<ArgumentException>();
            }
            finally
            {
                newPassword.Dispose();
                invalidContract.Dispose();
            }
        }

        [Test]
        public async Task ModifyComplementation_AddKeyFile_AllowsPasswordOrKeyFile()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var passwordProcedure = new AuthenticationMethod([AUTH_PASSWORD], null);
            var complementedProcedure = new AuthenticationMethod([AUTH_PASSWORD], AUTH_KEYFILE);

            using var password = await GetPasswordCreationCredentialAsync("Password#1");
            using var _ = await manager.CreateAsync(vaultFolder, password, CreateOptions(passwordProcedure, vaultId));

            using var unlockPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var unlockContract = await manager.UnlockAsync(vaultFolder, unlockPasskey);
            using var keyFile = await GetKeyFileCreationCredentialAsync(vaultId);

            // Act
            await manager.ModifyComplementationAsync(vaultFolder, unlockContract, new()
            {
                CurrentKeystoreCredential = unlockPasskey,
                CurrentPrimaryCredential = unlockPasskey,
                NewComplementCredential = keyFile
            }, CreateOptions(complementedProcedure, vaultId));

            // Assert
            using var passwordOnlyPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var keyFileOnlyPasskey = keyFile.CreateCopy();
            using var wrongPasswordPasskey = await GetPasswordLoginCredentialAsync("WrongPassword");
            using var chainedPasskey = new KeySequence();
            chainedPasskey.Add(await GetPasswordLoginCredentialAsync("Password#1"));
            chainedPasskey.Add(keyFile.CreateCopy());

            (await CanUnlockAsync(manager, vaultFolder, passwordOnlyPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, keyFileOnlyPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, chainedPasskey)).Should().BeFalse();
            (await CanUnlockAsync(manager, vaultFolder, wrongPasswordPasskey)).Should().BeFalse();

            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            var shares = await new VaultReader(vaultFolder, StreamSerializer.Instance).ReadComplementationAsync(CancellationToken.None);

            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(complementedProcedure);
            shares.Should().NotBeNull();
            shares!.Shares.Should().ContainSingle(x => x.AuthenticationMethodId == AUTH_KEYFILE);
        }

        [Test]
        public async Task ModifyComplementation_AddKeyFileFromChainedPasskey_UsesFullOldPasskey()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var chainedProcedure = new AuthenticationMethod([AUTH_PASSWORD, AUTH_KEYFILE], null);
            var complementedProcedure = new AuthenticationMethod([AUTH_PASSWORD], AUTH_KEYFILE);
            using var initialCompositePasskey = await GetCreationCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var _ = await manager.CreateAsync(vaultFolder, initialCompositePasskey, CreateOptions(chainedProcedure, vaultId));

            using var unlockPasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);
            using var unlockContract = await manager.UnlockAsync(vaultFolder, unlockPasskey);
            using var currentPrimary = await GetPasswordLoginCredentialAsync("Password#1");
            using var currentKeyFile = unlockPasskey.Keys.ElementAt(1).CreateCopy();

            // Act
            await manager.ModifyComplementationAsync(vaultFolder, unlockContract, new()
            {
                CurrentKeystoreCredential = unlockPasskey,
                CurrentPrimaryCredential = currentPrimary,
                NewComplementCredential = currentKeyFile
            }, CreateOptions(complementedProcedure, vaultId));

            // Assert
            using var passwordOnlyPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var keyFileOnlyPasskey = await GetKeyFileLoginCredentialAsync(vaultFolder, vaultId);
            using var oldChainedPasskey = await GetLoginCompositeCredentialAsync(vaultFolder, "Password#1", vaultId);

            (await CanUnlockAsync(manager, vaultFolder, passwordOnlyPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, keyFileOnlyPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, oldChainedPasskey)).Should().BeFalse();

            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            var shares = await new VaultReader(vaultFolder, StreamSerializer.Instance).ReadComplementationAsync(CancellationToken.None);

            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(complementedProcedure);
            shares.Should().NotBeNull();
            shares!.Shares.Should().ContainSingle(x => x.AuthenticationMethodId == AUTH_KEYFILE);
        }

        [Test]
        public async Task ModifyComplementation_ReplaceComplement_UsesNewComplementAndRejectsOld()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var biometricProcedure = new AuthenticationMethod([AUTH_PASSWORD], AUTH_APPLE_BIOMETRIC);
            using var oldKeyFile = await CreatePasswordVaultWithKeyFileComplementAsync(manager, vaultFolder, "Password#1", vaultId);

            // Replacing the complement rotates the complement secret to a new generation, which can only be
            // re-derived from the primary credential (the flow is therefore constrained to a primary login).
            using var unlockPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var unlockContract = await manager.UnlockAsync(vaultFolder, unlockPasskey);
            using var newComplement = SecureKey.CreateSecureRandom(32);

            // Act
            await manager.ModifyComplementationAsync(vaultFolder, unlockContract, new()
            {
                CurrentPrimaryCredential = unlockPasskey,
                NewComplementCredential = newComplement
            }, CreateOptions(biometricProcedure, vaultId));

            // Assert
            using var passwordOnlyPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var oldComplementPasskey = oldKeyFile.CreateCopy();
            using var newComplementPasskey = newComplement.CreateCopy();

            (await CanUnlockAsync(manager, vaultFolder, passwordOnlyPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, oldComplementPasskey)).Should().BeFalse();
            (await CanUnlockAsync(manager, vaultFolder, newComplementPasskey)).Should().BeTrue();

            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(biometricProcedure);
        }

        [Test]
        public async Task ModifyComplementation_ReplaceComplementWithoutPrimaryCredential_FailsAndPreservesOldCredentials()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var biometricProcedure = new AuthenticationMethod([AUTH_PASSWORD], AUTH_APPLE_BIOMETRIC);
            using var oldKeyFile = await CreatePasswordVaultWithKeyFileComplementAsync(manager, vaultFolder, "Password#1", vaultId);

            using var oldComplementUnlock = oldKeyFile.CreateCopy();
            using var unlockContract = await manager.UnlockAsync(vaultFolder, oldComplementUnlock);
            using var newComplement = SecureKey.CreateSecureRandom(32);

            // Act
            // Without the primary credential the complement secret cannot be rotated to a new generation.
            // Re-wrapping the old secret instead would let a revoked complement credential combined with an
            // old copy of the share file still unlock the vault, so the operation must be rejected.
            Func<Task> action = () => manager.ModifyComplementationAsync(vaultFolder, unlockContract, new()
            {
                CurrentComplementCredential = oldKeyFile,
                NewComplementCredential = newComplement
            }, CreateOptions(biometricProcedure, vaultId));

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Current primary credentials*");

            using var passwordOnlyPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var oldComplementPasskey = oldKeyFile.CreateCopy();
            using var newComplementPasskey = newComplement.CreateCopy();

            (await CanUnlockAsync(manager, vaultFolder, passwordOnlyPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, oldComplementPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, newComplementPasskey)).Should().BeFalse();
        }

        [Test]
        public async Task ModifyComplementation_RemoveComplement_RestoresPrimaryOnlyUnlock()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var passwordOnlyProcedure = new AuthenticationMethod([AUTH_PASSWORD], null);
            using var keyFile = await CreatePasswordVaultWithKeyFileComplementAsync(manager, vaultFolder, "Password#1", vaultId);

            using var unlockPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var unlockContract = await manager.UnlockAsync(vaultFolder, unlockPasskey);

            // Act
            await manager.ModifyComplementationAsync(vaultFolder, unlockContract, new()
            {
                CurrentPrimaryCredential = unlockPasskey
            }, CreateOptions(passwordOnlyProcedure, vaultId));

            // Assert
            using var passwordOnlyPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var keyFileOnlyPasskey = keyFile.CreateCopy();

            (await CanUnlockAsync(manager, vaultFolder, passwordOnlyPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, keyFileOnlyPasskey)).Should().BeFalse();

            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            var shares = await new VaultReader(vaultFolder, StreamSerializer.Instance).ReadComplementationAsync(CancellationToken.None);
            var complementFile = await vaultFolder.TryGetFileByNameAsync(SecureFolderFS.Core.Constants.Vault.Names.VAULT_COMPLEMENTATION_FILENAME);

            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(passwordOnlyProcedure);
            shares.Should().BeNull();
            complementFile.Should().BeNull();
        }

        [Test]
        public async Task ModifyComplementation_ChangePrimaryWithComplement_PreservesComplementUnlock()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var complementedProcedure = new AuthenticationMethod([AUTH_PASSWORD], AUTH_KEYFILE);
            using var keyFile = await CreatePasswordVaultWithKeyFileComplementAsync(manager, vaultFolder, "Password#1", vaultId);

            using var keyFileUnlock = keyFile.CreateCopy();
            using var unlockContract = await manager.UnlockAsync(vaultFolder, keyFileUnlock);
            using var newPassword = await GetPasswordCreationCredentialAsync("Password#2");

            // Act
            await manager.ModifyComplementationAsync(vaultFolder, unlockContract, new()
            {
                CurrentComplementCredential = keyFile,
                NewPrimaryCredential = newPassword
            }, CreateOptions(complementedProcedure, vaultId));

            // Assert
            using var oldPasswordPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var newPasswordPasskey = await GetPasswordLoginCredentialAsync("Password#2");
            using var keyFileOnlyPasskey = keyFile.CreateCopy();

            (await CanUnlockAsync(manager, vaultFolder, oldPasswordPasskey)).Should().BeFalse();
            (await CanUnlockAsync(manager, vaultFolder, newPasswordPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, keyFileOnlyPasskey)).Should().BeTrue();
        }

        [Test]
        public async Task ModifyComplementation_ChangePrimaryWithComplementUsingPrimaryCurrentCredential_PreservesComplementUnlock()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var complementedProcedure = new AuthenticationMethod([AUTH_PASSWORD], AUTH_KEYFILE);
            using var keyFile = await CreatePasswordVaultWithKeyFileComplementAsync(manager, vaultFolder, "Password#1", vaultId);

            using var currentPasswordUnlock = await GetPasswordLoginCredentialAsync("Password#1");
            using var unlockContract = await manager.UnlockAsync(vaultFolder, currentPasswordUnlock);
            using var currentPassword = await GetPasswordLoginCredentialAsync("Password#1");
            using var newPassword = await GetPasswordCreationCredentialAsync("Password#2");

            // Act
            await manager.ModifyComplementationAsync(vaultFolder, unlockContract, new()
            {
                CurrentPrimaryCredential = currentPassword,
                CurrentComplementCredential = keyFile,
                NewPrimaryCredential = newPassword
            }, CreateOptions(complementedProcedure, vaultId));

            // Assert
            using var oldPasswordPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var newPasswordPasskey = await GetPasswordLoginCredentialAsync("Password#2");
            using var keyFileOnlyPasskey = keyFile.CreateCopy();

            (await CanUnlockAsync(manager, vaultFolder, oldPasswordPasskey)).Should().BeFalse();
            (await CanUnlockAsync(manager, vaultFolder, newPasswordPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, keyFileOnlyPasskey)).Should().BeTrue();
        }

        [Test]
        public async Task ModifyComplementation_ChangePrimaryWithoutComplementCredential_FailsAndPreservesOldCredentials()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var complementedProcedure = new AuthenticationMethod([AUTH_PASSWORD], AUTH_KEYFILE);
            using var keyFile = await CreatePasswordVaultWithKeyFileComplementAsync(manager, vaultFolder, "Password#1", vaultId);

            using var currentPasswordUnlock = await GetPasswordLoginCredentialAsync("Password#1");
            using var unlockContract = await manager.UnlockAsync(vaultFolder, currentPasswordUnlock);
            using var currentPassword = await GetPasswordLoginCredentialAsync("Password#1");
            using var newPassword = await GetPasswordCreationCredentialAsync("Password#2");

            // Act
            Func<Task> action = () => manager.ModifyComplementationAsync(vaultFolder, unlockContract, new()
            {
                CurrentPrimaryCredential = currentPassword,
                NewPrimaryCredential = newPassword
            }, CreateOptions(complementedProcedure, vaultId));

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Current complement credentials*");

            using var oldPasswordPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var newPasswordPasskey = await GetPasswordLoginCredentialAsync("Password#2");
            using var keyFileOnlyPasskey = keyFile.CreateCopy();

            (await CanUnlockAsync(manager, vaultFolder, oldPasswordPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, newPasswordPasskey)).Should().BeFalse();
            (await CanUnlockAsync(manager, vaultFolder, keyFileOnlyPasskey)).Should().BeTrue();
        }

        [Test]
        public async Task CredentialsOverlay_FirstStageEditOnComplementedVault_RequiresComplementCredential()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultId = Guid.NewGuid().ToString("N");
            using var keyFile = await CreatePasswordVaultWithKeyFileComplementAsync(manager, vaultFolder, "Password#1", vaultId);
            using var overlayViewModel = new CredentialsOverlayViewModel(vaultFolder, "Vault", AuthenticationStage.FirstStageOnly);

            // Act
            await overlayViewModel.InitAsync(CancellationToken.None);

            // Assert
            var currentAuthentication = overlayViewModel.LoginViewModel.CurrentViewModel.Should().BeAssignableTo<AuthenticationViewModel>().Subject;

            currentAuthentication.Id.Should().Be(AUTH_KEYFILE);
            overlayViewModel.LoginViewModel.AuthenticationOptions.Should().ContainSingle(x => x.Id == AUTH_KEYFILE);
            overlayViewModel.LoginViewModel.AuthenticationOptions.Should().NotContain(x => x.Id == AUTH_PASSWORD);
        }

        [Test]
        public async Task CredentialsOverlay_ProceedingStageEditOnComplementedVault_RequiresPrimaryCredential()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultId = Guid.NewGuid().ToString("N");
            using var keyFile = await CreatePasswordVaultWithKeyFileComplementAsync(manager, vaultFolder, "Password#1", vaultId);
            using var overlayViewModel = new CredentialsOverlayViewModel(vaultFolder, "Vault", AuthenticationStage.ProceedingStageOnly);

            // Act
            await overlayViewModel.InitAsync(CancellationToken.None);

            // Assert
            var currentAuthentication = overlayViewModel.LoginViewModel.CurrentViewModel.Should().BeAssignableTo<AuthenticationViewModel>().Subject;

            currentAuthentication.Id.Should().Be(AUTH_PASSWORD);
            overlayViewModel.LoginViewModel.AuthenticationOptions.Should().ContainSingle(x => x.Id == AUTH_PASSWORD);
            overlayViewModel.LoginViewModel.AuthenticationOptions.Should().NotContain(x => x.Id == AUTH_KEYFILE);
        }

        [Test]
        public async Task ModifyComplementation_RemoveComplementWithoutPrimaryCredential_FailsAndPreservesComplement()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var manager = DI.Service<IVaultManagerService>();
            var vaultService = DI.Service<IVaultService>();
            var vaultId = Guid.NewGuid().ToString("N");

            var passwordOnlyProcedure = new AuthenticationMethod([AUTH_PASSWORD], null);
            var complementedProcedure = new AuthenticationMethod([AUTH_PASSWORD], AUTH_KEYFILE);
            using var keyFile = await CreatePasswordVaultWithKeyFileComplementAsync(manager, vaultFolder, "Password#1", vaultId);

            using var keyFileUnlock = keyFile.CreateCopy();
            using var unlockContract = await manager.UnlockAsync(vaultFolder, keyFileUnlock);
            using var currentComplement = keyFile.CreateCopy();

            // Act
            Func<Task> action = () => manager.ModifyComplementationAsync(vaultFolder, unlockContract, new()
            {
                CurrentComplementCredential = currentComplement
            }, CreateOptions(passwordOnlyProcedure, vaultId));

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Current primary credentials*");

            using var passwordOnlyPasskey = await GetPasswordLoginCredentialAsync("Password#1");
            using var keyFileOnlyPasskey = keyFile.CreateCopy();

            (await CanUnlockAsync(manager, vaultFolder, passwordOnlyPasskey)).Should().BeTrue();
            (await CanUnlockAsync(manager, vaultFolder, keyFileOnlyPasskey)).Should().BeTrue();

            var configuredOptions = await vaultService.GetVaultOptionsAsync(vaultFolder);
            configuredOptions.UnlockProcedure.Should().BeEquivalentTo(complementedProcedure);
        }

        [Test]
        public async Task FromUnlockProcedureAsync_DistinguishesChainedAndComplementedCredentials()
        {
            // Arrange
            var vaultFolder = CreateVaultFolder();
            var credentialsService = DI.Service<IVaultCredentialsService>();

            // Act
            var chainedText = await credentialsService.FromUnlockProcedureAsync(vaultFolder, new([AUTH_PASSWORD, AUTH_KEYFILE], null));
            var complementedText = await credentialsService.FromUnlockProcedureAsync(vaultFolder, new([AUTH_PASSWORD], AUTH_KEYFILE));

            // Assert
            chainedText.Should().Contain(" + ");
            chainedText.Should().NotContain(" / ");
            complementedText.Should().Contain(" / ");
            complementedText.Should().NotContain(" + ");
        }

        private static IFolder CreateVaultFolder()
        {
            var path = Path.Combine(Path.DirectorySeparatorChar.ToString(), $"TestVault-{Guid.NewGuid():N}");
            return new MemoryFolder(path, Path.GetFileName(path));
        }

        private static VaultOptions CreateOptions(AuthenticationMethod unlockProcedure, string vaultId)
        {
            return new VaultOptions()
            {
                UnlockProcedure = unlockProcedure,
                VaultId = vaultId
            };
        }

        private static async Task<IKeyUsage> GetPasswordCreationCredentialAsync(string password)
        {
            using var viewModel = CreatePasswordCreationViewModel(password);

            var result = await viewModel.EnrollAsync(Guid.NewGuid().ToString("N"), null);
            return result.TryGetValue(out var key)
                ? key
                : throw result.Exception ?? new InvalidOperationException("Password creation credential was not provided.");
        }

        private static async Task<IKeyUsage> GetPasswordLoginCredentialAsync(string password)
        {
            using var viewModel = CreatePasswordLoginViewModel(password);

            var result = await viewModel.AcquireAsync(Guid.NewGuid().ToString("N"), null);
            return result.TryGetValue(out var key)
                ? key
                : throw result.Exception ?? new InvalidOperationException("Password login credential was not provided.");
        }

        private static PasswordCreationViewModel CreatePasswordCreationViewModel(string password)
        {
            var viewModel = new PasswordCreationViewModel();
            viewModel.PrimaryPassword = password;
            viewModel.SecondaryPassword = password;

            return viewModel;
        }

        private static PasswordLoginViewModel CreatePasswordLoginViewModel(string password)
        {
            var viewModel = new PasswordLoginViewModel();
            viewModel.PrimaryPassword = password;

            return viewModel;
        }

        private static async Task<KeySequence> GetCreationCompositeCredentialAsync(IFolder vaultFolder, string password, string vaultId)
        {
            _ = vaultFolder;
            var keySequence = new KeySequence();

            try
            {
                keySequence.Add(await GetPasswordCreationCredentialAsync(password));
                keySequence.Add(await GetKeyFileCreationCredentialAsync(vaultId));

                return keySequence;
            }
            catch
            {
                keySequence.Dispose();
                throw;
            }
        }

        private static async Task<KeySequence> GetLoginCompositeCredentialAsync(IFolder vaultFolder, string password, string vaultId)
        {
            var keySequence = new KeySequence();

            try
            {
                keySequence.Add(await GetPasswordLoginCredentialAsync(password));
                keySequence.Add(await GetKeyFileLoginCredentialAsync(vaultFolder, vaultId));

                return keySequence;
            }
            catch
            {
                keySequence.Dispose();
                throw;
            }
        }

        private static async Task<IKeyUsage> GetKeyFileCreationCredentialAsync(string vaultId)
        {
            using var viewModel = new KeyFileCreationViewModel(vaultId);
            var result = await viewModel.EnrollAsync(vaultId, null);

            return result.TryGetValue(out var key)
                ? key
                : throw result.Exception ?? new InvalidOperationException("Key file creation credential was not provided.");
        }

        private static async Task<IKeyUsage> GetKeyFileLoginCredentialAsync(IFolder vaultFolder, string vaultId)
        {
            using var viewModel = new KeyFileLoginViewModel(vaultFolder);
            var result = await viewModel.AcquireAsync(vaultId, null);

            return result.TryGetValue(out var key)
                ? key
                : throw result.Exception ?? new InvalidOperationException("Key file login credential was not provided.");
        }

        private static async Task<IKeyUsage> CreatePasswordVaultWithKeyFileComplementAsync(IVaultManagerService manager, IFolder vaultFolder, string password, string vaultId)
        {
            var passwordProcedure = new AuthenticationMethod([AUTH_PASSWORD], null);
            var complementedProcedure = new AuthenticationMethod([AUTH_PASSWORD], AUTH_KEYFILE);

            using var passwordKey = await GetPasswordCreationCredentialAsync(password);
            using var _ = await manager.CreateAsync(vaultFolder, passwordKey, CreateOptions(passwordProcedure, vaultId));

            using var unlockPasskey = await GetPasswordLoginCredentialAsync(password);
            using var unlockContract = await manager.UnlockAsync(vaultFolder, unlockPasskey);
            var keyFile = await GetKeyFileCreationCredentialAsync(vaultId);

            try
            {
                await manager.ModifyComplementationAsync(vaultFolder, unlockContract, new()
                {
                    CurrentKeystoreCredential = unlockPasskey,
                    CurrentPrimaryCredential = unlockPasskey,
                    NewComplementCredential = keyFile
                }, CreateOptions(complementedProcedure, vaultId));

                return keyFile;
            }
            catch
            {
                keyFile.Dispose();
                throw;
            }
        }

        private static async Task<bool> CanUnlockAsync(IVaultManagerService manager, IFolder vaultFolder, IKeyUsage passkey)
        {
            try
            {
                using var _ = await manager.UnlockAsync(vaultFolder, passkey);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
