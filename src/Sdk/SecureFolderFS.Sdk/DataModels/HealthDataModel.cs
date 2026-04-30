using System;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed record class HealthDataModel(DateTime? LastScanDate, Severity? Severity);
}