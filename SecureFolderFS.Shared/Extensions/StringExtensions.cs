namespace SecureFolderFS.Shared.Extensions
{
    public static class StringExtensions
    {
        public static string FirstToUpper(this string str)
        {
            return $"{str[0].ToString().ToUpper()}{str.Substring(1, str.Length - 1)}";
        }
    }
}
