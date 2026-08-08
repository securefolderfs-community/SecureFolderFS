#if __UNO_SKIA_MACOS__
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Uno.PInvoke;
using static SecureFolderFS.Uno.PInvoke.UnsafeNative;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <summary>
    /// An <see cref="IPropertyStore{TKey}"/> backed by the macOS Keychain.
    /// </summary>
    internal sealed class MacOsKeychainPropertyStore : IPropertyStore<string>
    {
        /// <summary>Keychain service name all items are filed under.</summary>
        private const string ServiceName = "com.securefolderfs.deviceKeys";

        /// <summary>
        /// Probes the Keychain with a harmless query so callers can fall back when the native
        /// libraries are unavailable (should always succeed on macOS).
        /// </summary>
        internal static bool IsSupported()
        {
            try
            {
                _ = GetRaw("__sffs_probe__");
                return true;
            }
            catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public Task<TValue?> GetValueAsync<TValue>(string key, Func<TValue?>? defaultValue = null, CancellationToken cancellationToken = default)
        {
            var raw = GetRaw(key);
            if (raw is null)
                return Task.FromResult(defaultValue is not null ? defaultValue() : default);

            if (typeof(TValue) == typeof(string))
                return Task.FromResult((TValue?)(object)raw);

            return Task.FromResult(JsonSerializer.Deserialize<TValue>(raw));
        }

        /// <inheritdoc/>
        public Task<bool> SetValueAsync<TValue>(string key, TValue? value, CancellationToken cancellationToken = default)
        {
            var raw = value as string ?? JsonSerializer.Serialize(value);
            SetRaw(key, raw);
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            using var query = new CfDictionary(
                (MacOsConstants.SecClass, MacOsConstants.SecClassGenericPassword),
                (MacOsConstants.SecAttrService, CfString(ServiceName)),
                (MacOsConstants.SecAttrAccount, CfString(key)));

            return Task.FromResult(SecItemDelete(query.Handle) == ErrSecSuccess);
        }

        /// <inheritdoc/>
        public Task WipeAsync(CancellationToken cancellationToken = default)
        {
            // SecItemDelete removes every item matching the query.
            using var query = new CfDictionary(
                (MacOsConstants.SecClass, MacOsConstants.SecClassGenericPassword),
                (MacOsConstants.SecAttrService, CfString(ServiceName)));

            _ = SecItemDelete(query.Handle);
            return Task.CompletedTask;
        }

        private static string? GetRaw(string key)
        {
            using var query = new CfDictionary(
                (MacOsConstants.SecClass, MacOsConstants.SecClassGenericPassword),
                (MacOsConstants.SecAttrService, CfString(ServiceName)),
                (MacOsConstants.SecAttrAccount, CfString(key)),
                (MacOsConstants.SecReturnData, MacOsConstants.CfBooleanTrue),
                (MacOsConstants.SecMatchLimit, MacOsConstants.SecMatchLimitOne));

            var status = SecItemCopyMatching(query.Handle, out var result);
            if (status == ErrSecItemNotFound)
                return null;
            if (status != ErrSecSuccess)
                throw new InvalidOperationException($"Keychain read failed with status {status}.");

            try
            {
                var length = (int)CFDataGetLength(result);
                var bytes = new byte[length];
                Marshal.Copy(CFDataGetBytePtr(result), bytes, 0, length);
                return Encoding.UTF8.GetString(bytes);
            }
            finally
            {
                CFRelease(result);
            }
        }

        private static void SetRaw(string key, string value)
        {
            var data = Encoding.UTF8.GetBytes(value);

            using (var addQuery = new CfDictionary(
                (MacOsConstants.SecClass, MacOsConstants.SecClassGenericPassword),
                (MacOsConstants.SecAttrService, CfString(ServiceName)),
                (MacOsConstants.SecAttrAccount, CfString(key)),
                (MacOsConstants.SecValueData, CfData(data))))
            {
                var status = SecItemAdd(addQuery.Handle, IntPtr.Zero);
                if (status == ErrSecSuccess)
                    return;
                if (status != ErrSecDuplicateItem)
                    throw new InvalidOperationException($"Keychain write failed with status {status}.");
            }

            using var findQuery = new CfDictionary(
                (MacOsConstants.SecClass, MacOsConstants.SecClassGenericPassword),
                (MacOsConstants.SecAttrService, CfString(ServiceName)),
                (MacOsConstants.SecAttrAccount, CfString(key)));
            using var updateAttrs = new CfDictionary(
                (MacOsConstants.SecValueData, CfData(data)));

            var updateStatus = SecItemUpdate(findQuery.Handle, updateAttrs.Handle);
            if (updateStatus != ErrSecSuccess)
                throw new InvalidOperationException($"Keychain update failed with status {updateStatus}.");
        }
    }
}
#endif
