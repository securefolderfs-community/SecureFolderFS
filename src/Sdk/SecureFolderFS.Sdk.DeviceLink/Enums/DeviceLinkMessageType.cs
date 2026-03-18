namespace SecureFolderFS.Sdk.DeviceLink.Enums
{
    /// <summary>
    /// Message types for the DeviceLink protocol.
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        /// Discovery broadcast from desktop.
        /// </summary>
        DiscoveryRequest = 0x01,

        /// <summary>
        /// Discovery response from mobile.
        /// </summary>
        DiscoveryResponse = 0x02,

        /// <summary>
        /// Connection request from desktop.
        /// </summary>
        ConnectionRequest = 0x10,

        /// <summary>
        /// Connection accepted by mobile.
        /// </summary>
        ConnectionAccepted = 0x11,

        /// <summary>
        /// Connection rejected by mobile.
        /// </summary>
        ConnectionRejected = 0x12,

        /// <summary>
        /// Challenge sent from desktop to mobile.
        /// </summary>
        ChallengeRequest = 0x20,

        /// <summary>
        /// Signed challenge response from mobile.
        /// </summary>
        ChallengeResponse = 0x21,

        /// <summary>
        /// Request list of available credentials.
        /// </summary>
        ListCredentialsRequest = 0x30,

        /// <summary>
        /// List of credentials response.
        /// </summary>
        ListCredentialsResponse = 0x31,

        /// <summary>
        /// Select a specific credential to use.
        /// </summary>
        SelectCredential = 0x32,

        /// <summary>
        /// Credential selected confirmation.
        /// </summary>
        CredentialSelected = 0x33,

        // ============ Secure Pairing Protocol (0x40-0x4F) ============

        /// <summary>
        /// Initiate secure pairing from desktop.
        /// Contains: Desktop ECDH public key
        /// </summary>
        PairingRequest = 0x40,

        /// <summary>
        /// Pairing response from mobile.
        /// Contains: Mobile ECDH public key
        /// </summary>
        PairingResponse = 0x41,

        /// <summary>
        /// User confirmed verification code matches.
        /// Contains: VaultCID, VaultName
        /// </summary>
        PairingConfirm = 0x42,

        /// <summary>
        /// Pairing completed, credential created.
        /// Contains: Signing public key
        /// </summary>
        PairingComplete = 0x43,

        /// <summary>
        /// Pairing was rejected or verification failed.
        /// </summary>
        PairingRejected = 0x44,

        // ============ Secure Authentication Protocol (0x50-0x5F) ============

        /// <summary>
        /// Secure authentication request (encrypted payload).
        /// Contains: CID + Challenge + Timestamp + Nonce
        /// </summary>
        SecureAuthRequest = 0x50,

        /// <summary>
        /// Secure authentication response (encrypted payload).
        /// Contains: Signature
        /// </summary>
        SecureAuthResponse = 0x51,

        /// <summary>
        /// Secure session establishment using pairing credentials.
        /// Contains: PairingId + Session nonce
        /// </summary>
        SecureSessionRequest = 0x52,

        /// <summary>
        /// Secure session established.
        /// Contains: Session nonce response
        /// </summary>
        SecureSessionAccepted = 0x53,

        /// <summary>
        /// Authentication rejected (wrong CID, expired, etc.)
        /// </summary>
        AuthenticationRejected = 0x54,

        /// <summary>
        /// Error message.
        /// </summary>
        Error = 0xFF
    }
}