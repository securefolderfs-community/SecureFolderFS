using System;

namespace SecureFolderFS.Shared.Extensions
{
    public static class SpanExtensions
    {
        public static bool IsAllZeros(this ReadOnlySpan<byte> span)
        {
            foreach (var b in span)
            {
                if (b != 0)
                    return false;
            }

            return true;
        }
    }
}
