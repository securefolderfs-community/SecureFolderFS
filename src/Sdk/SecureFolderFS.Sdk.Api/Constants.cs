using System.Collections.Generic;

namespace SecureFolderFS.Sdk.Api
{
    public static class Constants
    {
        public const int PROTOCOL_VERSION = 1;
        public const int PROTOCOL_VERSION_MIN = 1;
        public const string ENDPOINT_FILE_NAME = "api-endpoint.json";
        public const string APP_DIRECTORY_NAME = "SecureFolderFS";
        public const string XDG_DIRECTORY_NAME = "securefolderfs";

        public static class Methods
        {
            public const string HELLO = "hello";
            public const string PAIR = "pair";
            public const string VAULTS_LIST = "vaults.list";
            public const string VAULTS_SUBSCRIBE = "vaults.subscribe";
            public const string VAULTS_UNSUBSCRIBE = "vaults.unsubscribe";
            public const string VAULTS_REQUEST_UNLOCK = "vaults.requestUnlock";
            public const string VAULTS_LOCK = "vaults.lock";
            public const string VAULTS_REVEAL = "vaults.reveal";
            public const string APP_SHOW = "app.show";
        }

        public static class Events
        {
            public const string VAULT_ADDED = "vault.added";
            public const string VAULT_REMOVED = "vault.removed";
            public const string VAULT_RENAMED = "vault.renamed";
            public const string VAULT_UNLOCKED = "vault.unlocked";
            public const string VAULT_LOCKED = "vault.locked";
        }

        public static class Scopes
        {
            /// <summary>Allows listing vaults and subscribing to changes.</summary>
            public const string VAULTS_READ = "vaults.read";

            /// <summary>Allows raising the unlock prompt, locking, and revealing mounted vaults.</summary>
            public const string VAULTS_TRIGGER = "vaults.trigger";

            /// <summary>Allows bringing the main window to the foreground.</summary>
            public const string APP_CONTROL = "app.control";

            public static IReadOnlyList<string> All { get; } = [VAULTS_READ, VAULTS_TRIGGER, APP_CONTROL];
        }

        public static class ErrorCodes
        {
            public const string PARSE_ERROR = "parse_error";
            public const string INVALID_REQUEST = "invalid_request";
            public const string UNKNOWN_METHOD = "unknown_method";
            public const string INVALID_PARAMS = "invalid_params";
            public const string UNAUTHORIZED = "unauthorized";
            public const string FORBIDDEN_SCOPE = "forbidden_scope";
            public const string RATE_LIMITED = "rate_limited";
            public const string PAIRING_REQUIRED = "pairing_required";
            public const string PAIRING_DENIED = "pairing_denied";
            public const string PAIRING_UNAVAILABLE = "pairing_unavailable";
            public const string NOT_FOUND = "not_found";
            public const string INVALID_STATE = "invalid_state";
            public const string UNSUPPORTED_VERSION = "unsupported_version";
            public const string INTERNAL_ERROR = "internal_error";
        }

        public static class VaultStates
        {
            public const string LOCKED = "locked";
            public const string UNLOCKED = "unlocked";
        }

        public static class ActionStatus
        {
            public const string OK = "ok";
            public const string ALREADY_PENDING = "already_pending";
            public const string NO_CHANGE = "no_change";
        }

        public static class SessionStates
        {
            public const string PAIRED = "paired";
            public const string PAIRING_REQUIRED = "pairing_required";
        }

        public static class Limits
        {
            public const int MAX_MESSAGE_BYTES = 64 * 1024;
            public const int MAX_CONNECTIONS = 16;
            public const int MAX_UNAUTHENTICATED_CONNECTIONS = 4;
            public const int HANDSHAKE_TIMEOUT_SECONDS = 30;
            public const int MAX_CLIENT_NAME_LENGTH = 64;
        }
    }
}
