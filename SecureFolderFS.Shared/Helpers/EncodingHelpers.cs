namespace SecureFolderFS.Shared.Helpers
{
    public static class EncodingHelpers
    {
        private static readonly char[] base64Padding = { '=' };

        public static string WithBase64UrlEncoding(string base64string)
        {
            return base64string.TrimEnd(base64Padding).Replace('+', '-').Replace('/', '_');
        }

        public static string WithoutBase64UrlEncoding(string base64Urlstring)
        {
            var base64string = base64Urlstring.Replace('_', '/').Replace('-', '+');
            return (base64string.Length % 4) switch
            {
                2 => base64string += "==",
                3 => base64string += "=",
                _ => base64string
            };
        }
    }
}
