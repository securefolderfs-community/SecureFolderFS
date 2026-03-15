#if __UNO_SKIA_MACOS__
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SecureFolderFS.Uno.PInvoke
{
    internal static partial class UnsafeNative
    {
        /// <summary>
        /// Provides low-level interop with macOS Security and LocalAuthentication frameworks
        /// for Secure Enclave key management and biometric authentication.
        /// </summary>
        internal static partial class Biometrics
        {
            // Security framework constants loaded at runtime via dlsym
            private static readonly IntPtr _kSecAttrKeyType;
            private static readonly IntPtr _kSecAttrKeyTypeECSECPrimeRandom;
            private static readonly IntPtr _kSecAttrKeySizeInBits;
            private static readonly IntPtr _kSecAttrTokenID;
            private static readonly IntPtr _kSecAttrTokenIDSecureEnclave;
            private static readonly IntPtr _kSecPrivateKeyAttrs;
            private static readonly IntPtr _kSecAttrIsPermanent;
            private static readonly IntPtr _kSecAttrApplicationTag;
            private static readonly IntPtr _kSecAttrAccessControl;
            private static readonly IntPtr _kSecClass;
            private static readonly IntPtr _kSecClassKey;
            private static readonly IntPtr _kSecAttrKeyClass;
            private static readonly IntPtr _kSecAttrKeyClassPrivate;
            private static readonly IntPtr _kSecReturnRef;
            private static readonly IntPtr _kSecMatchLimit;
            private static readonly IntPtr _kSecMatchLimitOne;
            private static readonly IntPtr _kSecKeyAlgorithmECIESEncryptionStandardX963SHA256AESGCM;

            // errSecSuccess
            private const int ErrSecSuccess = 0;

            // SecAccessControlCreateFlags
            private const ulong kSecAccessControlPrivateKeyUsage = 1 << 30;
            private const ulong kSecAccessControlUserPresence = 1;
            private const ulong kSecAccessControlBiometryAny = 2;

            // LAPolicy
            private const long LAPolicyDeviceOwnerAuthenticationWithBiometrics = 1;

            [DllImport("/usr/lib/libSystem.dylib")]
            private static extern IntPtr dlopen(string path, int mode);

            [DllImport("/usr/lib/libSystem.dylib")]
            private static extern IntPtr dlsym(IntPtr handle, string symbol);

            static Biometrics()
            {
                var securityLib = dlopen("/System/Library/Frameworks/Security.framework/Security", 1);

                _kSecAttrKeyType = ReadGlobalCFString(securityLib, "kSecAttrKeyType");
                _kSecAttrKeyTypeECSECPrimeRandom = ReadGlobalCFString(securityLib, "kSecAttrKeyTypeECSECPrimeRandom");
                _kSecAttrKeySizeInBits = ReadGlobalCFString(securityLib, "kSecAttrKeySizeInBits");
                _kSecAttrTokenID = ReadGlobalCFString(securityLib, "kSecAttrTokenID");
                _kSecAttrTokenIDSecureEnclave = ReadGlobalCFString(securityLib, "kSecAttrTokenIDSecureEnclave");
                _kSecPrivateKeyAttrs = ReadGlobalCFString(securityLib, "kSecPrivateKeyAttrs");
                _kSecAttrIsPermanent = ReadGlobalCFString(securityLib, "kSecAttrIsPermanent");
                _kSecAttrApplicationTag = ReadGlobalCFString(securityLib, "kSecAttrApplicationTag");
                _kSecAttrAccessControl = ReadGlobalCFString(securityLib, "kSecAttrAccessControl");
                _kSecClass = ReadGlobalCFString(securityLib, "kSecClass");
                _kSecClassKey = ReadGlobalCFString(securityLib, "kSecClassKey");
                _kSecAttrKeyClass = ReadGlobalCFString(securityLib, "kSecAttrKeyClass");
                _kSecAttrKeyClassPrivate = ReadGlobalCFString(securityLib, "kSecAttrKeyClassPrivate");
                _kSecReturnRef = ReadGlobalCFString(securityLib, "kSecReturnRef");
                _kSecMatchLimit = ReadGlobalCFString(securityLib, "kSecMatchLimit");
                _kSecMatchLimitOne = ReadGlobalCFString(securityLib, "kSecMatchLimitOne");

                // Load the algorithm constant for ECIES encryption
                var algSymbol = dlsym(securityLib, "kSecKeyAlgorithmECIESEncryptionStandardX963SHA256AESGCM");
                if (algSymbol != IntPtr.Zero)
                    _kSecKeyAlgorithmECIESEncryptionStandardX963SHA256AESGCM = Marshal.ReadIntPtr(algSymbol);
            }

            private static IntPtr ReadGlobalCFString(IntPtr lib, string symbol)
            {
                var ptr = dlsym(lib, symbol);
                if (ptr == IntPtr.Zero)
                    return IntPtr.Zero;

                return Marshal.ReadIntPtr(ptr);
            }

            #region LAContext Biometric Evaluation

            /// <summary>
            /// Checks whether biometric authentication is available on this device.
            /// </summary>
            public static bool CanEvaluateBiometricPolicy()
            {
                var laContextClass = objc_getClass("LAContext");
                var allocSel = sel_registerName("alloc");
                var initSel = sel_registerName("init");

                var context = objc_msgSend_IntPtr(laContextClass, allocSel);
                context = objc_msgSend_IntPtr(context, initSel);

                if (context == IntPtr.Zero)
                    return false;

                try
                {
                    var canEvalSel = sel_registerName("canEvaluatePolicy:error:");
                    var errPtr = IntPtr.Zero;
                    var result = objc_msgSend_bool_long_IntPtr(context, canEvalSel, LAPolicyDeviceOwnerAuthenticationWithBiometrics, errPtr);
                    return result;
                }
                finally
                {
                    var releaseSel = sel_registerName("release");
                    objc_msgSend_IntPtr(context, releaseSel);
                }
            }

            /// <summary>
            /// Evaluates biometric policy asynchronously using LAContext.
            /// </summary>
            /// <param name="reason">The localized reason string shown to the user.</param>
            /// <returns>True if authentication succeeded.</returns>
            public static Task<bool> EvaluateBiometricPolicyAsync(string reason)
            {
                var tcs = new TaskCompletionSource<bool>();

                var laContextClass = objc_getClass("LAContext");
                var allocSel = sel_registerName("alloc");
                var initSel = sel_registerName("init");

                var context = objc_msgSend_IntPtr(laContextClass, allocSel);
                context = objc_msgSend_IntPtr(context, initSel);

                if (context == IntPtr.Zero)
                {
                    tcs.SetResult(false);
                    return tcs.Task;
                }

                // Check if policy can be evaluated
                var canEvalSel = sel_registerName("canEvaluatePolicy:error:");
                var canEval = objc_msgSend_bool_long_IntPtr(context, canEvalSel, LAPolicyDeviceOwnerAuthenticationWithBiometrics, IntPtr.Zero);
                if (!canEval)
                {
                    var releaseSel = sel_registerName("release");
                    objc_msgSend_IntPtr(context, releaseSel);
                    tcs.SetResult(false);
                    return tcs.Task;
                }

                // Create NSString for the reason
                var nsStringClass = objc_getClass("NSString");
                var stringWithUtf8Sel = sel_registerName("stringWithUTF8String:");
                var reasonPtr = Marshal.StringToCoTaskMemUTF8(reason);
                var nsReason = objc_msgSend_IntPtr_IntPtr(nsStringClass, stringWithUtf8Sel, reasonPtr);
                Marshal.FreeCoTaskMem(reasonPtr);

                // Create the reply block
                // The block must match: void (^)(BOOL success, NSError *error)
                var evaluateSel = sel_registerName("evaluatePolicy:localizedReason:reply:");

                // Use a delegate pinned in memory for the block callback
                BlockCallback callback = (blockPtr, success, error) =>
                {
                    tcs.TrySetResult(success != 0);
                };

                var block = CreateBlock(callback);

                objc_msgSend_void_long_IntPtr_IntPtr(context, evaluateSel, LAPolicyDeviceOwnerAuthenticationWithBiometrics, nsReason, block);

                // Release context after evaluation completes
                tcs.Task.ContinueWith(_ =>
                {
                    ReleaseBlock(block);
                    var releaseSel = sel_registerName("release");
                    objc_msgSend_IntPtr(context, releaseSel);
                });

                return tcs.Task;
            }

            #endregion

            #region Secure Enclave Key Management

            /// <summary>
            /// Creates a new EC key pair in the Secure Enclave.
            /// </summary>
            /// <param name="alias">The application tag for the key.</param>
            /// <returns>The private key handle.</returns>
            public static IntPtr CreateSecureEnclaveKey(string alias)
            {
                // Create SecAccessControl
                var access = CreateSecAccessControl();

                // Create the application tag as CFData
                var tagData = StringToCFData(alias);

                // Build private key attributes dictionary
                //   kSecAttrIsPermanent: true
                //   kSecAttrApplicationTag: tagData
                //   kSecAttrAccessControl: access
                var privateKeyAttrs = CreateCFDictionary(
                    new[] { _kSecAttrIsPermanent, _kSecAttrApplicationTag, _kSecAttrAccessControl },
                    new[] { GetCFBooleanTrueValue(), tagData, access },
                    3);

                // Build generation parameters dictionary
                //   kSecAttrKeyType: kSecAttrKeyTypeECSECPrimeRandom
                //   kSecAttrKeySizeInBits: 256
                //   kSecAttrTokenID: kSecAttrTokenIDSecureEnclave
                //   kSecPrivateKeyAttrs: privateKeyAttrs
                var keySizeNumber = CreateCFNumber(256);
                var parameters = CreateCFDictionary(
                    new[] { _kSecAttrKeyType, _kSecAttrKeySizeInBits, _kSecAttrTokenID, _kSecPrivateKeyAttrs },
                    new[] { _kSecAttrKeyTypeECSECPrimeRandom, keySizeNumber, _kSecAttrTokenIDSecureEnclave, privateKeyAttrs },
                    4);

                var privateKey = SecKeyCreateRandomKey(parameters, out var error);

                // Cleanup
                CFRelease(parameters);
                CFRelease(privateKeyAttrs);
                CFRelease(tagData);
                CFRelease(keySizeNumber);
                if (access != IntPtr.Zero)
                    CFRelease(access);

                if (privateKey == IntPtr.Zero || error != IntPtr.Zero)
                {
                    var errorDesc = GetNSErrorDescription(error);
                    if (error != IntPtr.Zero) CFRelease(error);
                    throw new CryptographicException($"Secure Enclave key creation failed. {errorDesc}");
                }

                return privateKey;
            }

            /// <summary>
            /// Queries the Keychain for an existing private key by alias.
            /// </summary>
            /// <param name="alias">The application tag for the key.</param>
            /// <returns>The private key handle, or IntPtr.Zero if not found.</returns>
            public static IntPtr GetPrivateKey(string alias)
            {
                var tagData = StringToCFData(alias);

                var query = CreateCFDictionary(
                    new[] { _kSecClass, _kSecAttrApplicationTag, _kSecAttrKeyClass, _kSecAttrTokenID, _kSecReturnRef, _kSecMatchLimit },
                    new[] { _kSecClassKey, tagData, _kSecAttrKeyClassPrivate, _kSecAttrTokenIDSecureEnclave, GetCFBooleanTrueValue(), _kSecMatchLimitOne },
                    6);

                var status = SecItemCopyMatching(query, out var result);

                CFRelease(query);
                CFRelease(tagData);

                return status == ErrSecSuccess ? result : IntPtr.Zero;
            }

            /// <summary>
            /// Deletes a key from the Keychain by alias.
            /// </summary>
            /// <param name="alias">The application tag for the key.</param>
            public static void DeleteKey(string alias)
            {
                var tagData = StringToCFData(alias);

                var query = CreateCFDictionary(
                    new[] { _kSecClass, _kSecAttrApplicationTag, _kSecAttrKeyClass, _kSecAttrTokenID },
                    new[] { _kSecClassKey, tagData, _kSecAttrKeyClassPrivate, _kSecAttrTokenIDSecureEnclave },
                    4);

                SecItemDelete(query);

                CFRelease(query);
                CFRelease(tagData);
            }

            /// <summary>
            /// Encrypts data using the public key derived from a Secure Enclave private key.
            /// </summary>
            /// <param name="privateKey">The private key handle.</param>
            /// <param name="plaintext">The data to encrypt.</param>
            /// <returns>The encrypted data.</returns>
            public static byte[] Encrypt(IntPtr privateKey, byte[] plaintext)
            {
                var publicKey = SecKeyCopyPublicKey(privateKey);
                if (publicKey == IntPtr.Zero)
                    throw new CryptographicException("Could not retrieve the public key from the private key.");

                try
                {
                    var cfPlaintext = ByteArrayToCFData(plaintext);
                    var cfCiphertext = SecKeyCreateEncryptedData(
                        publicKey,
                        _kSecKeyAlgorithmECIESEncryptionStandardX963SHA256AESGCM,
                        cfPlaintext,
                        out var error);

                    CFRelease(cfPlaintext);

                    if (cfCiphertext == IntPtr.Zero || error != IntPtr.Zero)
                    {
                        var errorDesc = GetNSErrorDescription(error);
                        if (error != IntPtr.Zero) CFRelease(error);
                        throw new CryptographicException($"Encryption failed. {errorDesc}");
                    }

                    var result = CFDataToByteArray(cfCiphertext);
                    CFRelease(cfCiphertext);
                    return result;
                }
                finally
                {
                    CFRelease(publicKey);
                }
            }

            /// <summary>
            /// Decrypts data using the Secure Enclave private key (triggers biometric prompt).
            /// </summary>
            /// <param name="privateKey">The private key handle.</param>
            /// <param name="ciphertext">The data to decrypt.</param>
            /// <returns>The decrypted data.</returns>
            public static byte[] Decrypt(IntPtr privateKey, byte[] ciphertext)
            {
                var cfCiphertext = ByteArrayToCFData(ciphertext);
                var cfPlaintext = SecKeyCreateDecryptedData(
                    privateKey,
                    _kSecKeyAlgorithmECIESEncryptionStandardX963SHA256AESGCM,
                    cfCiphertext,
                    out var error);

                CFRelease(cfCiphertext);

                if (cfPlaintext == IntPtr.Zero || error != IntPtr.Zero)
                {
                    var errorDesc = GetNSErrorDescription(error);
                    if (error != IntPtr.Zero) CFRelease(error);
                    throw new CryptographicException($"Decryption failed. {errorDesc}");
                }

                var result = CFDataToByteArray(cfPlaintext);
                CFRelease(cfPlaintext);
                return result;
            }

            #endregion

            #region Helpers

            private static IntPtr GetCFBooleanTrueValue()
            {
                var cfLib = dlopen("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", 1);
                var ptr = dlsym(cfLib, "kCFBooleanTrue");
                return Marshal.ReadIntPtr(ptr);
            }

            [LibraryImport("/System/Library/Frameworks/Security.framework/Security")]
            private static partial IntPtr SecAccessControlCreateWithFlags(
                IntPtr allocator,
                IntPtr protection,
                ulong flags,
                out IntPtr error);

            private static IntPtr CreateSecAccessControl()
            {
                // kSecAttrAccessibleWhenUnlockedThisDeviceOnly
                var secLib = dlopen("/System/Library/Frameworks/Security.framework/Security", 1);
                var protectionPtr = dlsym(secLib, "kSecAttrAccessibleWhenUnlockedThisDeviceOnly");
                var protection = Marshal.ReadIntPtr(protectionPtr);

                var flags = kSecAccessControlPrivateKeyUsage | kSecAccessControlBiometryAny;
                var access = SecAccessControlCreateWithFlags(IntPtr.Zero, protection, flags, out var error);

                if (access == IntPtr.Zero || error != IntPtr.Zero)
                {
                    var errorDesc = GetNSErrorDescription(error);
                    if (error != IntPtr.Zero) CFRelease(error);
                    throw new CryptographicException($"SecAccessControl creation failed. {errorDesc}");
                }

                return access;
            }

            private static IntPtr StringToCFData(string value)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(value);
                return ByteArrayToCFData(bytes);
            }

            private static IntPtr ByteArrayToCFData(byte[] data)
            {
                var pinned = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    return CFDataCreate(IntPtr.Zero, pinned.AddrOfPinnedObject(), data.Length);
                }
                finally
                {
                    pinned.Free();
                }
            }

            private static byte[] CFDataToByteArray(IntPtr cfData)
            {
                var length = (int)CFDataGetLength(cfData);
                var ptr = CFDataGetBytePtr(cfData);
                var result = new byte[length];
                Marshal.Copy(ptr, result, 0, length);
                return result;
            }

            private static IntPtr CreateCFNumber(int value)
            {
                // kCFNumberSInt32Type = 3
                var pinned = GCHandle.Alloc(value, GCHandleType.Pinned);
                try
                {
                    return CFNumberCreate(IntPtr.Zero, 3, pinned.AddrOfPinnedObject());
                }
                finally
                {
                    pinned.Free();
                }
            }

            private static IntPtr CreateCFDictionary(IntPtr[] keys, IntPtr[] values, int count)
            {
                var keysHandle = GCHandle.Alloc(keys, GCHandleType.Pinned);
                var valuesHandle = GCHandle.Alloc(values, GCHandleType.Pinned);

                try
                {
                    // Get the default callbacks
                    var cfLib = dlopen("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", 1);
                    var keyCallbacksPtr = dlsym(cfLib, "kCFTypeDictionaryKeyCallBacks");
                    var valueCallbacksPtr = dlsym(cfLib, "kCFTypeDictionaryValueCallBacks");

                    return CFDictionaryCreate(
                        IntPtr.Zero,
                        keysHandle.AddrOfPinnedObject(),
                        valuesHandle.AddrOfPinnedObject(),
                        count,
                        keyCallbacksPtr,
                        valueCallbacksPtr);
                }
                finally
                {
                    keysHandle.Free();
                    valuesHandle.Free();
                }
            }

            private static string? GetNSErrorDescription(IntPtr nsError)
            {
                if (nsError == IntPtr.Zero)
                    return null;

                var descSel = sel_registerName("localizedDescription");
                var nsString = objc_msgSend_IntPtr(nsError, descSel);
                if (nsString == IntPtr.Zero)
                    return null;

                var utf8Sel = sel_registerName("UTF8String");
                var utf8Ptr = objc_msgSend_IntPtr(nsString, utf8Sel);
                return utf8Ptr != IntPtr.Zero ? Marshal.PtrToStringUTF8(utf8Ptr) : null;
            }

            #endregion

            #region Objective-C Block Support

            // Delegate matching the LAContext evaluatePolicy reply: void(^)(BOOL, NSError*)
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void BlockCallback(IntPtr block, byte success, IntPtr error);

            [StructLayout(LayoutKind.Sequential)]
            private struct BlockLiteral
            {
                public IntPtr isa;
                public int flags;
                public int reserved;
                public IntPtr invoke;
                public IntPtr descriptor;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct BlockDescriptor
            {
                public ulong reserved;
                public ulong size;
            }

            private static IntPtr CreateBlock(BlockCallback callback)
            {
                // _NSConcreteStackBlock - we use global block instead for safety
                var classPtr = dlsym(dlopen("/usr/lib/libobjc.A.dylib", 1), "_NSConcreteGlobalBlock");

                var descriptor = new BlockDescriptor
                {
                    reserved = 0,
                    size = (ulong)Marshal.SizeOf<BlockLiteral>()
                };
                var descriptorPtr = Marshal.AllocHGlobal(Marshal.SizeOf<BlockDescriptor>());
                Marshal.StructureToPtr(descriptor, descriptorPtr, false);

                var fnPtr = Marshal.GetFunctionPointerForDelegate(callback);

                var block = new BlockLiteral
                {
                    isa = classPtr,
                    flags = 1 << 28, // BLOCK_HAS_COPY_DISPOSE not set, IS_GLOBAL = 1 << 28
                    reserved = 0,
                    invoke = fnPtr,
                    descriptor = descriptorPtr
                };

                var blockPtr = Marshal.AllocHGlobal(Marshal.SizeOf<BlockLiteral>());
                Marshal.StructureToPtr(block, blockPtr, false);

                // Pin the callback delegate to prevent GC collection
                GCHandle.Alloc(callback);

                return blockPtr;
            }

            private static void ReleaseBlock(IntPtr blockPtr)
            {
                if (blockPtr == IntPtr.Zero)
                    return;

                var block = Marshal.PtrToStructure<BlockLiteral>(blockPtr);
                if (block.descriptor != IntPtr.Zero)
                    Marshal.FreeHGlobal(block.descriptor);

                Marshal.FreeHGlobal(blockPtr);
            }

            #endregion
        }
    }
}
#endif
