using Android.Hardware.Biometrics;
using Java.Lang;

namespace SecureFolderFS.Maui.AppModels
{
    /// <inheritdoc cref="BiometricPrompt.AuthenticationCallback"/>
    internal sealed class BiometricPromptCallback : BiometricPrompt.AuthenticationCallback
    {
        private readonly Action<BiometricPrompt.AuthenticationResult?>? _onSuccess;
        private readonly Action<BiometricErrorCode, string?>? _onError;
        private readonly Action? _onFailure;

        public BiometricPromptCallback(
            Action<BiometricPrompt.AuthenticationResult?>? onSuccess,
            Action<BiometricErrorCode, string?>? onError,
            Action? onFailure)
        {
            _onSuccess = onSuccess;
            _onError = onError;
            _onFailure = onFailure;
        }

        /// <inheritdoc/>
        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult? result)
        {
            _onSuccess?.Invoke(result);
        }

        /// <inheritdoc/>
        public override void OnAuthenticationError(BiometricErrorCode errorCode, ICharSequence? errString)
        {
            _onError?.Invoke(errorCode, errString?.ToString());
        }

        /// <inheritdoc/>
        public override void OnAuthenticationFailed()
        {
            _onFailure?.Invoke();
        }
    }
}
