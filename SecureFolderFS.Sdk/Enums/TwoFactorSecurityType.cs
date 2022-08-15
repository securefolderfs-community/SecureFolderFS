using System;

namespace SecureFolderFS.Sdk.Enums
{
    [Serializable]
    public enum TwoFactorSecurityType : uint
    {
        None = 0,
        PlatformUnlock = 1,
        UniversalU2fKey = 2,
        ManualProvide = 4
    }
}