namespace SecureFolderFS.Sdk.Enums
{
    /// <summary>
    /// Represents types of in-app purchase (IAP) products available within the application.
    /// </summary>
    public enum IapProductType
    {
        /// <summary>
        /// Represents the one-time purchase option for acquiring the lifetime license.
        /// </summary>
        PlusLifetime = 0,

        /// <summary>
        /// Represents the subscription-based option for accessing premium features.
        /// </summary>
        PlusSubscription = 1,

        /// <summary>
        /// Represents any of the available IAP products.
        /// </summary>
        Any = PlusLifetime | PlusSubscription
    }
}
