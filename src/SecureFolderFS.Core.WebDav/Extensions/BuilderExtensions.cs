using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.Store;

namespace SecureFolderFS.Core.WebDav.Extensions
{
    internal static class BuilderExtensions
    {
        public static IServiceCollection AddCipherStore(this IServiceCollection services, Action<CipherStoreOptions> options)
        {
            return services
                .Configure(options)
                .AddSingleton<DiskStoreItemPropertyManager>()
                .AddSingleton<DiskStoreCollectionPropertyManager>()
                .AddScoped<IStore, CipherStore>(sp =>
                {
                    var storeOptions = sp.GetService<IOptions<CipherStoreOptions>>();
                    if (storeOptions?.Value.Specifics is null)
                        throw new NotSupportedException("Options were not configured.");

                    var itemPropertyManager = sp.GetService<DiskStoreItemPropertyManager>() ?? throw new ArgumentNullException(nameof(DiskStoreItemPropertyManager));
                    var collectionPropertyManager = sp.GetService<DiskStoreCollectionPropertyManager>() ?? throw new ArgumentNullException(nameof(DiskStoreCollectionPropertyManager));
                    var logger = sp.GetService<ILogger>() ?? throw new ArgumentNullException(nameof(ILogger));

                    return new(storeOptions.Value.Specifics, itemPropertyManager, collectionPropertyManager, logger);
                });
        }
    }
}
