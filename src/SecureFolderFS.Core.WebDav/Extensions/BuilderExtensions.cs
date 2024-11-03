using Microsoft.Extensions.DependencyInjection;
using NWebDav.Server;
using NWebDav.Server.Stores;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Core.WebDav.EncryptingStorage2;
using System;
using Microsoft.Extensions.Options;

namespace SecureFolderFS.Core.WebDav.Extensions
{
    internal static class BuilderExtensions
    {
        public static IServiceCollection AddEncryptingDiskStore(this IServiceCollection services, Action<EncryptingStoreOptions> options)
        {
            return services
                .Configure(options)
                .AddSingleton<DiskStoreCollectionPropertyManager>()
                .AddSingleton<DiskStoreItemPropertyManager>()
                .AddScoped<IStore, EncryptingDiskStore>(sp =>
                {
                    var storeOptions = sp.GetService<IOptions<EncryptingStoreOptions>>();
                    if (storeOptions?.Value.Specifics is null)
                        throw new NotSupportedException("Options were not configured.");

                    return new(storeOptions.Value.Specifics);

                });
        }
    }
}
