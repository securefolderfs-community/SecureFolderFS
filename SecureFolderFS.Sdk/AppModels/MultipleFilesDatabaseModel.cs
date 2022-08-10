using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="BaseDictionaryDatabaseModel"/>
    public sealed class MultipleFilesDatabaseModel : BaseDictionaryDatabaseModel
    {
        private readonly IModifiableFolder _databaseFolder;

        public MultipleFilesDatabaseModel(IModifiableFolder databaseFolder, IAsyncSerializer<Stream> serializer)
            : base(serializer)
        {
        }

        /// <inheritdoc/>
        public override async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();

            //try
            //{
            //    await storageSemaphore.WaitAsync(cancellationToken);
            //    settingsCache.Clear();

            //    await foreach (var item in _databaseFolder.GetFilesAsync(cancellationToken))
            //    {
            //        serializer.DeserializeAsync<object>()
            //        settingsCache[item.Name]
            //    }
            //}
            //finally
            //{
            //    _ = storageSemaphore.Release();
            //}
        }

        /// <inheritdoc/>
        public override Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
