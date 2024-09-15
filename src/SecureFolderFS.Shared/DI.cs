using System;

namespace SecureFolderFS.Shared
{
    /// <summary>
    /// A type that represents a dependency injection service resolver that implements <see cref="IServiceProvider"/>.
    /// </summary>
    public sealed class DI : IServiceProvider
    {
        private volatile IServiceProvider? _serviceProvider;

        /// <summary>
        /// Gets the default instance of <see cref="DI"/>.
        /// </summary>
        public static DI Default { get; } = new();

        /// <summary>
        /// Gets the value that indicates whether the <see cref="IServiceProvider"/> is ready to be used.
        /// </summary>
        public bool IsAvailable => _serviceProvider is not null;

        /// <inheritdoc/>
        public object? GetService(Type serviceType)
        {
            var serviceProvider = _serviceProvider;
            _ = serviceProvider ?? throw new InvalidOperationException("The IServiceProvider has not been configured yet.");

            return serviceProvider.GetService(serviceType);
        }

        /// <summary>
        /// Resolves a specific service identified by <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>An instance of the specified service.</returns>
        public T GetService<T>()
            where T : class
        {
            return (T?)GetService(typeof(T)) ?? throw new InvalidOperationException($"The requested {typeof(T)} service was not registered.");
        }

        /// <summary>
        /// Resolves a specific (optional) service identified by <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>An instance of the specified service, or null.</returns>
        public T? GetOptionalService<T>()
            where T : class
        {
            if (_serviceProvider is null)
                return default;
            
            return (T?)GetService(typeof(T));
        }

        /// <summary>
        /// Resolves a specific service identified by <typeparamref name="T"/> from <see cref="DI.Default"/>.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>An instance of the specified service.</returns>
        public static T Service<T>()
            where T : class
        {
            return Default.GetService<T>();
        }

        /// <summary>
        /// Resolves a specific (optional) service identified by <typeparamref name="T"/> from <see cref="DI.Default"/>.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns>An instance of the specified service, or null.</returns>
        public static T? OptionalService<T>()
            where T : class
        {
            return Default.GetOptionalService<T>();
        }

        /// <summary>
        /// Sets the current <see cref="IServiceProvider"/> source.
        /// </summary>
        /// <param name="serviceProvider">The source service resolver.</param>
        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    }
}
