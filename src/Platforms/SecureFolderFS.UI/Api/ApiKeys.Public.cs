namespace SecureFolderFS.UI.Api
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

        static partial void RetrieveSentryDsnKey(ref string? key);
    }
}
