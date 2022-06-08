namespace SecureFolderFS.Sdk.Enums
{
    public enum AppUpdateResult
    {
        Completed = 0,
        InProgress = 1,
        Canceled = 2,
        FailedNetworkError = 4,
        FailedDeviceError = 8,
        FailedUnknownError = 16
    }
}
