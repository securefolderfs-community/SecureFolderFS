namespace SecureFolderFS.UI.Api
{
    public static partial class ApiKeys
    {
        public static string? GetAppCenterKey()
        {
            string? key = null;
            AppCenterKey(ref key);

            return key;
        }

        static partial void AppCenterKey(ref string? key);
    }
}
