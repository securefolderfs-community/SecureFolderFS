namespace SecureFolderFS.Shared.Api
{
    public static partial class ApiKeys
    {
        public static string? SentryDsnKey
        {
            get
            {
                string? key = null;
                RetrieveSentryDsnKey(ref key);

                return key;
            }
        }

        public static string? GoogleDriveClientKey
        {
            get
            {
                string? key = null;
                RetrieveGoogleDriveClientKey(ref key);

                return key;
            }
        }

        static partial void RetrieveSentryDsnKey(ref string? key);

        static partial void RetrieveGoogleDriveClientKey(ref string? key);
    }
}
