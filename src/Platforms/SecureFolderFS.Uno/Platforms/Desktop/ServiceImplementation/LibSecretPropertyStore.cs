#if !__UNO_SKIA_MACOS__ && !WINDOWS
using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;
using static SecureFolderFS.Uno.PInvoke.UnsafeNative;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <summary>
    /// An <see cref="IPropertyStore{TKey}"/> backed by the freedesktop Secret Service via libsecret
    /// (GNOME Keyring, KWallet 5.97+, etc.). Values are protected by the user's login keyring.
    /// </summary>
    internal sealed partial class LibSecretPropertyStore : IPropertyStore<string>, IDisposable
    {
        private const string SchemaName = "com.securefolderfs.deviceKeys";

        private readonly IntPtr _schema;

        public LibSecretPropertyStore()
        {
            _schema = CreateSchema();
        }

        /// <summary>
        /// Probes for a usable Secret Service so callers can fall back when libsecret or the
        /// user session's keyring daemon is unavailable.
        /// </summary>
        internal static bool IsSupported()
        {
            try
            {
                using var store = new LibSecretPropertyStore();
                _ = store.GetRaw("__sffs_probe__");
                return true;
            }
            catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or InvalidOperationException)
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

            using var attributes = new SecretAttributes(key);
            var stored = secret_password_storev_sync(
                _schema, attributes.Handle, SecretCollectionDefault, $"SecureFolderFS ({key})", raw, IntPtr.Zero, out var error);

            ThrowOnGError(error, "store");
            return Task.FromResult(stored != 0);
        }

        /// <inheritdoc/>
        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            using var attributes = new SecretAttributes(key);
            var cleared = secret_password_clearv_sync(_schema, attributes.Handle, IntPtr.Zero, out var error);

            ThrowOnGError(error, "clear");
            return Task.FromResult(cleared != 0);
        }

        /// <inheritdoc/>
        public Task WipeAsync(CancellationToken cancellationToken = default)
        {
            // An empty attribute set matches every item of the schema.
            using var attributes = new SecretAttributes(key: null);
            _ = secret_password_clearv_sync(_schema, attributes.Handle, IntPtr.Zero, out var error);

            ThrowOnGError(error, "wipe");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_schema != IntPtr.Zero)
                Marshal.FreeHGlobal(_schema);
        }

        private string? GetRaw(string key)
        {
            using var attributes = new SecretAttributes(key);
            var result = secret_password_lookupv_sync(_schema, attributes.Handle, IntPtr.Zero, out var error);

            ThrowOnGError(error, "lookup");
            if (result == IntPtr.Zero)
                return null;

            try
            {
                return Marshal.PtrToStringUTF8(result);
            }
            finally
            {
                secret_password_free(result);
            }
        }

        private static void ThrowOnGError(IntPtr error, string operation)
        {
            if (error == IntPtr.Zero)
                return;

            // GError layout: { GQuark domain; gint code; gchar* message; }
            var messagePtr = Marshal.ReadIntPtr(error, IntPtr.Size);
            var message = messagePtr != IntPtr.Zero ? Marshal.PtrToStringUTF8(messagePtr) : null;
            g_error_free(error);

            throw new InvalidOperationException($"Secret Service {operation} failed: {message ?? "unknown error"}.");
        }

        /// <summary>
        /// Builds an unmanaged SecretSchema with a single string attribute ("key").
        /// Layout: { const gchar* name; int flags; SecretSchemaAttribute attributes[32]; ... }
        /// where SecretSchemaAttribute is { const gchar* name; int type; }. The terminating
        /// attribute entry has a null name. The strings and struct live for the process lifetime.
        /// </summary>
        private static IntPtr CreateSchema()
        {
            var attributeEntrySize = IntPtr.Size + IntPtr.Size; // pointer + int (padded to pointer size)
            var headerSize = IntPtr.Size + IntPtr.Size;         // name pointer + flags (padded)
            var schema = Marshal.AllocHGlobal(headerSize + 32 * attributeEntrySize);

            // Zero the whole block so unused attribute slots terminate the list.
            for (var offset = 0; offset < headerSize + 32 * attributeEntrySize; offset += IntPtr.Size)
                Marshal.WriteIntPtr(schema, offset, IntPtr.Zero);

            Marshal.WriteIntPtr(schema, 0, Marshal.StringToHGlobalAnsi(SchemaName));
            Marshal.WriteInt32(schema, IntPtr.Size, 0 /* SECRET_SCHEMA_NONE */);

            // attributes[0] = { "key", SECRET_SCHEMA_ATTRIBUTE_STRING (0) }
            Marshal.WriteIntPtr(schema, headerSize, Marshal.StringToHGlobalAnsi("Key"));
            Marshal.WriteInt32(schema, headerSize + IntPtr.Size, 0);

            return schema;
        }
    }
}
#endif
