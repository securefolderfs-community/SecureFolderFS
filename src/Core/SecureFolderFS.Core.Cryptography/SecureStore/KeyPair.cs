using System;
using SecureFolderFS.Core.Cryptography.Extensions;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Cryptography.SecureStore
{
    /// <summary>
    /// Represents a chain of secret keys used for encryption and message authentication.
    /// </summary>
    public sealed class KeyPair : IDisposable
    {
        /// <summary>
        /// Gets the Data Encryption Key (DEK).
        /// </summary>
        public IKeyUsage DekKey { get; }

        /// <summary>
        /// Gets the Message Authentication Code (MAC) key.
        /// </summary>
        public IKeyUsage MacKey { get; }

        private KeyPair(IKeyUsage dekKey, IKeyUsage macKey)
        {
            DekKey = dekKey;
            MacKey = macKey;
        }

        /// <summary>
        /// Allows secure access to the DEK and MAC keys through the provided delegate.
        /// </summary>
        /// <param name="keyAction">A delegate that processes the DEK key and MAC key as read-only spans of bytes.</param>
        /// <remarks>
        /// The method securely provides access to the underlying DEK and MAC keys by passing them as read-only spans to the supplied delegate.
        /// Ensure the provided delegate does not retain references to the keys outside its execution scope.
        /// </remarks>
        public unsafe void UseKeys(Action<ReadOnlySpan<byte>, ReadOnlySpan<byte>> keyAction)
        {
            DekKey.UseKey(dekKey =>
            {
                fixed (byte* dekPtr = dekKey)
                {
                    var state = (dekPtr: (nint)dekPtr, dekLen: dekKey.Length);
                    MacKey.UseKey(state, (mac, s) =>
                    {
                        var dek = new ReadOnlySpan<byte>((byte*)s.dekPtr, s.dekLen);
                        keyAction(dek, mac);
                    });
                }
            });
        }

        /// <inheritdoc cref="UseKeys"/>
        public unsafe void UseKeys<TState>(TState state, Action<ReadOnlySpan<byte>, ReadOnlySpan<byte>, TState> keyAction)
        {
            DekKey.UseKey(dekKey =>
            {
                fixed (byte* dekPtr = dekKey)
                {
                    var innerState = (dekPtr: (nint)dekPtr, dekLen: dekKey.Length, outerState: state, action: keyAction);
                    MacKey.UseKey(innerState, (mac, s) =>
                    {
                        var dek = new ReadOnlySpan<byte>((byte*)s.dekPtr, s.dekLen);
                        s.action(dek, mac, s.outerState);
                    });
                }
            });
        }

        /// <summary>
        /// Allows secure execution of a function that processes the DEK and MAC keys as read-only spans of bytes and returns a result.
        /// </summary>
        /// <param name="keyAction">A delegate that processes the DEK key and MAC key as read-only spans of bytes.</param>
        /// <typeparam name="TResult">The type of the result produced by the function.</typeparam>
        /// <returns>The result produced by executing the provided function with the DEK and MAC keys.</returns>
        /// <remarks>
        /// The method securely provides access to the underlying DEK and MAC keys by passing them as read-only spans to the supplied delegate.
        /// Ensure the provided delegate does not retain references to the keys outside its execution scope.
        /// </remarks>
        public unsafe TResult UseKeys<TResult>(Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, TResult> keyAction)
        {
            return DekKey.UseKey(dekKey =>
            {
                fixed (byte* dekPtr = dekKey)
                {
                    var state = (dekPtr: (nint)dekPtr, dekLen: dekKey.Length, action: keyAction);
                    return MacKey.UseKey(state, (mac, s) =>
                    {
                        var dek = new ReadOnlySpan<byte>((byte*)s.dekPtr, s.dekLen);
                        return s.action(dek, mac);
                    });
                }
            });
        }

        /// <inheritdoc cref="UseKeys"/>
        public unsafe TResult UseKeys<TState, TResult>(TState state, Func<ReadOnlySpan<byte>, ReadOnlySpan<byte>, TState, TResult> keyAction)
        {
            return DekKey.UseKey(dekKey =>
            {
                fixed (byte* dekPtr = dekKey)
                {
                    var innerState = (dekPtr: (nint)dekPtr, dekLen: dekKey.Length, outerState: state, action: keyAction);
                    return MacKey.UseKey(innerState, (mac, s) =>
                    {
                        var dek = new ReadOnlySpan<byte>((byte*)s.dekPtr, s.dekLen);
                        return s.action(dek, mac, s.outerState);
                    });
                }
            });
        }

        /// <summary>
        /// Creates a new copy of the current <see cref="KeyPair"/> instance, including separate copies of the contained DEK and MAC keys.
        /// </summary>
        /// <returns>A new <see cref="KeyPair"/> instance with copied DEK and MAC keys.</returns>
        public KeyPair CreateCopy()
        {
            return new(DekKey.CreateCopy(), MacKey.CreateCopy());
        }

        /// <summary>
        /// Imports the specified DEK and MAC keys, creating unique copies of them and disposing the original instances.
        /// </summary>
        /// <param name="dekKeyToDestroy">The DEK to import.</param>
        /// <param name="macKeyToDestroy">The MAC key to import.</param>
        /// <remarks>
        /// This method copies the imported keys and disposed of the original instances.
        /// Make sure no other classes access the passed keys after they are imported.
        /// Instead, use <see cref="DekKey"/> and <see cref="MacKey"/> instances.
        /// </remarks>
        /// <returns>A new instance of the <see cref="KeyPair"/> class with the imported keys.</returns>
        public static KeyPair ImportKeys(IKeyUsage dekKeyToDestroy, IKeyUsage macKeyToDestroy)
        {
            return new(dekKeyToDestroy.CreateUniqueCopy(), macKeyToDestroy.CreateUniqueCopy());
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return UseKeys((dekKey, macKey) =>
            {
                return $"{Convert.ToBase64String(dekKey)}{Constants.KeyTraits.KEY_TEXT_SEPARATOR}{Convert.ToBase64String(macKey)}";
            });
        }

        /// <summary>
        /// Combines the provided encoded recovery key into a <see cref="ManagedKey"/> instance.
        /// </summary>
        /// <param name="encodedRecoveryKey">The Base64 encoded recovery key.</param>
        /// <returns>A <see cref="ManagedKey"/> instance representing the combined recovery key.</returns>
        public static ManagedKey CombineRecoveryKey(string encodedRecoveryKey)
        {
            var keySplit = encodedRecoveryKey.ReplaceLineEndings(string.Empty).Split(Constants.KeyTraits.KEY_TEXT_SEPARATOR);
            using var recoveryKey = new ManagedKey(Constants.KeyTraits.DEK_KEY_LENGTH + Constants.KeyTraits.MAC_KEY_LENGTH);

            if (!Convert.TryFromBase64String(keySplit[0], recoveryKey.Key.AsSpan(0, Constants.KeyTraits.DEK_KEY_LENGTH), out _))
                throw new FormatException("The recovery key (1) was not in the correct format.");

            if (!Convert.TryFromBase64String(keySplit[1], recoveryKey.Key.AsSpan(Constants.KeyTraits.DEK_KEY_LENGTH, Constants.KeyTraits.MAC_KEY_LENGTH), out _))
                throw new FormatException("The recovery key (2) was not in the correct format.");

            return recoveryKey.CreateCopy();
        }

        /// <summary>
        /// Creates a <see cref="KeyPair"/> from the specified recovery key.
        /// </summary>
        /// <param name="recoveryKey">The combined recovery key.</param>
        /// <returns>A <see cref="KeyPair"/> instance representing the key pair.</returns>
        public static KeyPair CopyFromRecoveryKey(ManagedKey recoveryKey)
        {
            var dekKey = new byte[Constants.KeyTraits.DEK_KEY_LENGTH];
            var macKey = new byte[Constants.KeyTraits.MAC_KEY_LENGTH];

            recoveryKey.Key.AsSpan(0, Constants.KeyTraits.DEK_KEY_LENGTH).CopyTo(dekKey);
            recoveryKey.Key.AsSpan(Constants.KeyTraits.DEK_KEY_LENGTH, Constants.KeyTraits.MAC_KEY_LENGTH).CopyTo(macKey);

            return new KeyPair(SecureKey.TakeOwnership(dekKey), SecureKey.TakeOwnership(macKey));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DekKey.Dispose();
            MacKey.Dispose();
        }
    }
}
