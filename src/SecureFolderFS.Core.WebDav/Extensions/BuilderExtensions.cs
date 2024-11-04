using Microsoft.Extensions.DependencyInjection;
using NWebDav.Server;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.EncryptingStorage2;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SecureFolderFS.Core.WebDav.Extensions
{
    internal static class BuilderExtensions
    {
        public static IServiceCollection AddEncryptingDiskStore(this IServiceCollection services, Action<EncryptingStoreOptions> options)
        {
            return services
                .Configure(options)
                .AddSingleton<DiskStoreItemPropertyManager>()
                .AddSingleton<DiskStoreCollectionPropertyManager>()
                .AddScoped<IStore, EncryptingDiskStore>(sp =>
                {
                    var storeOptions = sp.GetService<IOptions<EncryptingStoreOptions>>();
                    if (storeOptions?.Value.Specifics is null)
                        throw new NotSupportedException("Options were not configured.");

                    var itemPropertyManager = sp.GetService<DiskStoreItemPropertyManager>() ?? throw new ArgumentNullException(nameof(DiskStoreItemPropertyManager));
                    var collectionPropertyManager = sp.GetService<DiskStoreCollectionPropertyManager>() ?? throw new ArgumentNullException(nameof(DiskStoreCollectionPropertyManager));
                    var loggerFactory = sp.GetService<ILoggerFactory>() ?? throw new ArgumentNullException(nameof(ILoggerFactory));

                    return new(storeOptions.Value.Specifics, itemPropertyManager, collectionPropertyManager, loggerFactory);

                });
        }
    }
}
