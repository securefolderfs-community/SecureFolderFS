﻿using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultUnlockingModel"/>
    [Inject<IVaultService>, Inject<ISettingsService>]
    public sealed partial class VaultUnlockingModel : IVaultUnlockingModel
    {
        public VaultUnlockingModel()
        {
            ServiceProvider = Ioc.Default;
        }

        /// <inheritdoc/>
        public async Task<IResult> SetFolderAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            //// TODO: Maybe use IAsyncValidator<IFolder>
            //var vaultFolderResult = await VaultUnlockingService.SetVaultFolderAsync(folder, cancellationToken);
            //if (!vaultFolderResult.Successful)
            //    return vaultFolderResult;

            //var configFileResult = await folder.GetFileWithResultAsync(VaultService.ConfigurationFileName, cancellationToken);
            //if (!configFileResult.Successful)
            //    return configFileResult;

            //var configStreamResult = await configFileResult.Value!.OpenStreamWithResultAsync(FileAccess.Read, FileShare.Read, cancellationToken);
            //if (!configStreamResult.Successful)
            //    return configStreamResult;

            //await using (configStreamResult.Value)
            //{
            //    var setStreamResult = await VaultUnlockingService.SetConfigurationStreamAsync(configStreamResult.Value!, cancellationToken);
            //    if (!setStreamResult.Successful)
            //        return setStreamResult;
            //}

            //return CommonResult.Success;
            return CommonResult.Success;
        }

        /// <inheritdoc/>
        public async Task<IResult> SetKeystoreAsync(IKeystoreModel keystoreModel, CancellationToken cancellationToken = default)
        {
            //var keystoreStreamResult = await keystoreModel.GetKeystoreStreamAsync(FileAccess.Read, cancellationToken);
            //if (!keystoreStreamResult.Successful)
            //    return keystoreStreamResult;

            //return await VaultUnlockingService.SetKeystoreStreamAsync(keystoreStreamResult.Value!, keystoreModel.KeystoreSerializer, cancellationToken);
            return CommonResult.Success;

        }

        /// <inheritdoc/>
        public async Task<IResult<IVaultLifecycle?>> UnlockAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            //// Get file system
            //var fileSystem = VaultService.GetFileSystems().FirstOrDefault(x => x.Id == SettingsService.UserSettings.PreferredFileSystemId);
            //if (fileSystem is null)
            //    return new CommonResult<IVaultLifetimeModel?>(new ArgumentException($"File System descriptor '{SettingsService.UserSettings.PreferredFileSystemId}' was not found."));

            //var supportedResult = await fileSystem.GetStatusAsync(cancellationToken);
            //if (!supportedResult.Successful)
            //    return new CommonResult<IVaultLifetimeModel?>(supportedResult.Exception);

            //var fileSystemResult = await VaultUnlockingService.SetFileSystemAsync(fileSystem, cancellationToken);
            //if (!fileSystemResult.Successful)
            //    return new CommonResult<IVaultLifetimeModel?>(fileSystemResult.Exception);

            //return await VaultUnlockingService.UnlockAndStartAsync(password, cancellationToken);
            return null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
