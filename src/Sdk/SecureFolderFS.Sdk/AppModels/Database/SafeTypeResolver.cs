using System;
using System.Linq;

namespace SecureFolderFS.Sdk.AppModels.Database
{
    /// <summary>
    /// Resolves <see cref="Type"/> instances from persisted type strings while restricting resolution
    /// to a set of trusted assemblies.
    /// </summary>
    /// <remarks>
    /// The type name originates from an on-disk settings file. Resolving arbitrary assembly-qualified names would
    /// let anyone able to write to the settings folder coerce the app into loading unexpected types. Only the
    /// application's own assemblies and the core framework are considered safe.
    /// </remarks>
    internal static class SafeTypeResolver
    {
        private static readonly string[] AllowedAssemblyPrefixes =
        [
            nameof(SecureFolderFS),
            nameof(System),
            "mscorlib",
            "netstandard"
        ];

        /// <summary>
        /// Resolves the <see cref="Type"/> described by <paramref name="typeName"/> if it belongs to a trusted assembly.
        /// </summary>
        /// <param name="typeName">The assembly-qualified or namespace-qualified type name.</param>
        /// <returns>The resolved <see cref="Type"/>, or null if the type is unknown or resides in an untrusted assembly.</returns>
        public static Type? Resolve(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            var type = Type.GetType(typeName, throwOnError: false);
            if (type is null)
                return null;

            return IsTrusted(type) ? type : null;
        }

        private static bool IsTrusted(Type type)
        {
            // Validate the type itself and every generic argument
            var assemblyName = type.Assembly.GetName().Name;
            if (assemblyName is null || !AllowedAssemblyPrefixes.Any(prefix => assemblyName.StartsWith(prefix, StringComparison.Ordinal)))
                return false;

            if (type.IsGenericType)
                return type.GetGenericArguments().All(IsTrusted);

            return true;
        }
    }
}
