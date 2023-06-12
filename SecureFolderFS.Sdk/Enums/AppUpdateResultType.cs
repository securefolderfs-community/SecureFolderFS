namespace SecureFolderFS.Sdk.Enums
{
    /// <summary>
    /// Contains values for different update states.
    /// </summary>
    public enum AppUpdateResultType
    {
        FailedDeviceError = -8,
        FailedUnknownError = -4,
        FailedNetworkError = -2,
        Canceled = -1,
        None = 0,
        Completed = 1,
        InProgress = 2,
    }
}
