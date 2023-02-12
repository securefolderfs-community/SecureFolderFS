using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Extensions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IWidgetsContextModel"/>
    public sealed class SavedWidgetsContextModel : IWidgetsContextModel
    {
        private readonly IFolder _vaultFolder;

        private IVaultWidgets VaultWidgets { get; } = Ioc.Default.GetRequiredService<IVaultPersistenceService>().VaultWidgets;

        public SavedWidgetsContextModel(IFolder vaultFolder)
        {
            _vaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public async Task<bool> AddWidgetAsync(string widgetId, CancellationToken cancellationToken = default)
        {
            var widgetsContext = VaultWidgets.GetWidgetsContextForId(_vaultFolder.Id);
            widgetsContext.WidgetDataModels.AddOrReplace(widgetId, new());

            return await VaultWidgets.SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveWidgetAsync(string widgetId, CancellationToken cancellationToken = default)
        {
            var widgetsContext = VaultWidgets.GetWidgetsContextForId(_vaultFolder.Id);
            widgetsContext.WidgetDataModels.Remove(widgetId);

            return await VaultWidgets.SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IWidgetModel> GetWidgetsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var widgetsContext = VaultWidgets.GetWidgetsContextForId(_vaultFolder.Id);
            foreach (var item in widgetsContext.WidgetDataModels)
            {
                yield return new LocalWidgetModel(item.Key, VaultWidgets, item.Value);
            }
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await VaultWidgets.LoadAsync(cancellationToken);
        }
    }
}
