using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace SecureFolderFS.Shared.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection Override<TService, TImplementation>(
            this IServiceCollection services,
            Func<IServiceCollection, Type, Type, IServiceCollection> add)
            where TImplementation : class, TService
        {
            var serviceType = typeof(TService);
            var implementationType = typeof(TImplementation);
            var found = services.FirstOrDefault(x => x.ServiceType == serviceType);
            if (found is not null)
                services.Remove(found);

            return add(services, serviceType, implementationType);
        }

        public static IServiceCollection Override<TService, TImplementation>(
            this IServiceCollection services,
            Func<IServiceCollection, Type, Func<IServiceProvider, TImplementation>, IServiceCollection> add,
            Func<IServiceProvider, TImplementation> implementationFactory)
            where TImplementation : class, TService
        {
            var serviceType = typeof(TService);
            var found = services.FirstOrDefault(x => x.ServiceType == serviceType);
            if (found is not null)
                services.Remove(found);

            return add(services, serviceType, implementationFactory);
        }

        public static IServiceCollection Foundation<TService, TImplementation>(
            this IServiceCollection services,
            Func<IServiceCollection, Type, Type, IServiceCollection> add)
            where TImplementation : class, TService
        {
            var serviceType = typeof(TService);
            var implementationType = typeof(TImplementation);
            if (services.Any(x => x.ServiceType == serviceType))
                return services;

            return add(services, serviceType, implementationType);
        }

        public static IServiceCollection Foundation<TService, TImplementation>(
            this IServiceCollection services,
            Func<IServiceCollection, Type, Func<IServiceProvider, TImplementation>, IServiceCollection> add,
            Func<IServiceProvider, TImplementation> implementationFactory)
            where TImplementation : class, TService
        {
            var serviceType = typeof(TService);
            if (services.Any(x => x.ServiceType == serviceType))
                return services;

            return add(services, serviceType, implementationFactory);
        }
    }
}
