using FluentAssertions;
using NUnit.Framework;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
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

