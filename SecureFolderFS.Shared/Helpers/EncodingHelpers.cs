namespace SecureFolderFS.Shared.Helpers
{
    public static class EncodingHelpers
    {
        public static string EncodeBaseUrl64(string str)
        {
            var trimmedStr = str.TrimEnd('=');
            return trimmedStr.Replace('/', '_').Replace('+', '-');
        }

        public static string DecodeBaseUrl64(string encoded)
        {
            var decoded = encoded.Replace('_', '/').Replace('-', '+');
            return (decoded.Length % 4) switch
            {
                2 => decoded + "==",
                3 => decoded + "=",
                _ => decoded
            };
        }
    }
}
